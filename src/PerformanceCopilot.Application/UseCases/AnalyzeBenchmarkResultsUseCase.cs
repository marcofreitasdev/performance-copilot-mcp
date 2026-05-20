using PerformanceCopilot.Application.Contracts.Requests;
using PerformanceCopilot.Application.Contracts.Responses;
using PerformanceCopilot.Application.Services;
using PerformanceCopilot.Benchmarking.Models;
using PerformanceCopilot.Reporting.Models;

namespace PerformanceCopilot.Application.UseCases;

public sealed class AnalyzeBenchmarkResultsUseCase
{
    private readonly BenchmarkResultParser _parser;

    public AnalyzeBenchmarkResultsUseCase(BenchmarkResultParser parser)
    {
        _parser = parser;
    }

    public AnalyzeBenchmarkResultsResponse Execute(
        AnalyzeBenchmarkResultsRequest request,
        BenchmarkExecutionResult executionResult)
    {
        List<BenchmarkMethodResult> parsedResults;

        if (executionResult.Artifacts.BenchmarkDotNetJsonPath is not null)
        {
            parsedResults = _parser.ParseFromFile(
                executionResult.Artifacts.BenchmarkDotNetJsonPath,
                executionResult.BenchmarkRunId);
        }
        else if (executionResult.Results.Count > 0)
        {
            parsedResults = executionResult.Results;
        }
        else
        {
            var rawOutputPath = Path.Combine(executionResult.Artifacts.RootPath, "raw-output.txt");
            parsedResults = _parser.ParseFromRawOutput(rawOutputPath, executionResult.BenchmarkRunId);
        }

        var findings = parsedResults.Select(AnalyzeMethod).ToList();

        var primaryBottleneck = findings
            .OrderByDescending(f => SeverityScore(f.Severity))
            .FirstOrDefault()?.Category ?? "none";

        return new AnalyzeBenchmarkResultsResponse
        {
            BenchmarkRunId = request.BenchmarkRunId,
            Findings = findings,
            OverallAssessment = new OverallAssessment
            {
                Status = findings.Any(f => f.Severity == "high") ? "needs_attention" : "acceptable",
                PrimaryBottleneck = primaryBottleneck,
                RecommendedNextAction = BuildNextAction(primaryBottleneck)
            }
        };
    }

    private static TechnicalFinding AnalyzeMethod(BenchmarkMethodResult result)
    {
        var m = result.Metrics;

        string category;
        string severity;

        if (m.AllocatedBytes > 500_000 || m.Gen0Collections > 10)
        {
            category = "memory_allocation";
            severity = "high";
        }
        else if (m.AllocatedBytes > 100_000 || m.Gen0Collections > 3)
        {
            category = "memory_allocation";
            severity = "medium";
        }
        else if (m.MeanNs > 100_000_000)
        {
            category = "cpu_time";
            severity = "high";
        }
        else if (m.MeanNs > 10_000_000)
        {
            category = "cpu_time";
            severity = "medium";
        }
        else
        {
            category = "acceptable";
            severity = "low";
        }

        return new TechnicalFinding
        {
            Method = result.MethodUnderTest,
            Severity = severity,
            Category = category,
            Description = $"Method shows {DescribeCategory(category, m)}.",
            Evidence = new Dictionary<string, string>
            {
                ["mean"] = m.MeanReadable,
                ["allocated"] = m.AllocatedReadable,
                ["gen0"] = m.Gen0Collections.ToString(),
                ["opsPerSecond"] = $"{m.OperationsPerSecond:F2}"
            },
            PossibleCauses = BuildCauses(category),
            OptimizationSuggestions = BuildSuggestions(category)
        };
    }

    private static string DescribeCategory(string category, BenchmarkMetric m) => category switch
    {
        "memory_allocation" => $"high allocation per operation ({m.AllocatedReadable})",
        "cpu_time" => $"high average execution time ({m.MeanReadable})",
        _ => "performance within acceptable limits"
    };

    private static List<string> BuildCauses(string category) => category switch
    {
        "memory_allocation" =>
        [
            "Object allocation inside hot loops",
            "Repeated LINQ materialization",
            "Intensive string processing"
        ],
        "cpu_time" =>
        [
            "Computationally intensive logic",
            "High-complexity algorithm",
            "Unnecessary waiting or retries without backoff"
        ],
        _ => []
    };

    private static List<string> BuildSuggestions(string category) => category switch
    {
        "memory_allocation" =>
        [
            "Avoid ToList or ToArray in hot paths",
            "Preallocate List<T> when the size is known",
            "Consider ArrayPool<T> for reusable buffers",
            "Reduce repeated string normalization"
        ],
        "cpu_time" =>
        [
            "Review algorithmic complexity",
            "Consider caching repeated results",
            "Parallelize with PLINQ when safe"
        ],
        _ => ["No action required for now."]
    };

    private static string BuildNextAction(string bottleneck) => bottleneck switch
    {
        "memory_allocation" => "Optimize allocations before focusing on CPU time.",
        "cpu_time" => "Review the algorithm; CPU reductions may also improve memory usage.",
        _ => "Keep monitoring performance periodically."
    };

    private static int SeverityScore(string severity) => severity switch
    {
        "high" => 3,
        "medium" => 2,
        "low" => 1,
        _ => 0
    };
}
