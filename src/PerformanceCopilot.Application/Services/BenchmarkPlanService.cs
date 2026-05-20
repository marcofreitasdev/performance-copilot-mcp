using PerformanceCopilot.Application.Contracts.Requests;
using PerformanceCopilot.Application.Contracts.Responses;
using PerformanceCopilot.Benchmarking.Models;

namespace PerformanceCopilot.Application.Services;

public sealed class BenchmarkPlanService
{
    public BenchmarkTarget BuildTarget(BenchmarkCandidate candidate, GenerateBenchmarkPlanRequest request)
    {
        var job = request.BenchmarkMode switch
        {
            "default" => "Default",
            "medium" => "MediumRun",
            _ => "ShortRun"
        };

        var diagnosers = new List<string>();
        if (request.IncludeMemoryDiagnoser) diagnosers.Add("MemoryDiagnoser");

        var exporters = new List<string> { "Csv", "Html" };
        if (request.IncludeMarkdownExporter) exporters.Add("Markdown");
        if (request.IncludeJsonExporter) exporters.Add("Json");

        var difficulty = EstimateDifficulty(candidate);
        var fixtureStrategy = candidate.TechnicalClassification.RequiresFixture
            ? "generated_fake_data"
            : "none";

        return new BenchmarkTarget
        {
            CandidateId = candidate.CandidateId,
            BenchmarkClassName = $"{candidate.ClassName}Benchmark",
            BenchmarkMethodName = $"{candidate.MethodName}_Benchmark",
            BenchmarkType = candidate.SuggestedBenchmarkType,
            RequiredFixture = candidate.TechnicalClassification.RequiresFixture,
            FixtureStrategy = fixtureStrategy,
            EstimatedDifficulty = difficulty,
            RecommendedJob = job,
            Diagnosers = diagnosers,
            Exporters = exporters
        };
    }

    private static string EstimateDifficulty(BenchmarkCandidate candidate)
    {
        if (candidate.TechnicalClassification.HasExternalDependency) return "hard";
        if (candidate.TechnicalClassification.RequiresFixture) return "medium";
        return "easy";
    }
}
