using PerformanceCopilot.Application.Contracts.Requests;
using PerformanceCopilot.Application.Contracts.Responses;
using PerformanceCopilot.Benchmarking.Models;
using PerformanceCopilot.Reporting.Markdown;
using PerformanceCopilot.Reporting.Models;
using PerformanceCopilot.Storage.Local;

namespace PerformanceCopilot.Application.UseCases;

public sealed class GeneratePerformanceReportUseCase
{
    private readonly MarkdownReportGenerator _markdownGenerator;
    private readonly BenchmarkHistoryStore _historyStore;

    public GeneratePerformanceReportUseCase(
        MarkdownReportGenerator markdownGenerator,
        BenchmarkHistoryStore historyStore)
    {
        _markdownGenerator = markdownGenerator;
        _historyStore = historyStore;
    }

    public async Task<GeneratePerformanceReportResponse> ExecuteAsync(
        GeneratePerformanceReportRequest request,
        BenchmarkExecutionResult executionResult,
        AnalyzeBenchmarkResultsResponse analysis,
        CancellationToken ct = default)
    {
        var report = BuildReport(executionResult, analysis);
        var markdownContent = _markdownGenerator.Generate(report);

        var reportPath = executionResult.Artifacts.TechnicalReportPath;
        var reportDir = Path.GetDirectoryName(reportPath);
        if (reportDir is not null)
            Directory.CreateDirectory(reportDir);

        await File.WriteAllTextAsync(reportPath, markdownContent, ct);
        await _historyStore.SaveAsync(executionResult, ct);

        return new GeneratePerformanceReportResponse
        {
            Generated = true,
            ReportPath = reportPath,
            Sections = ["Environment", "Summary", "Technical Findings", "Optimization Recommendations"]
        };
    }

    private static PerformanceReport BuildReport(
        BenchmarkExecutionResult result,
        AnalyzeBenchmarkResultsResponse analysis) =>
        new()
        {
            BenchmarkRunId = result.BenchmarkRunId,
            Environment = result.Environment,
            Summary = new ReportSummary
            {
                Rows = result.Results.Count > 0
                    ? result.Results.Select(r => new MethodSummaryRow
                    {
                        Method = r.MethodUnderTest,
                        Mean = r.Metrics.MeanReadable,
                        Median = r.Metrics.MedianReadable,
                        Allocated = r.Metrics.AllocatedReadable,
                        Gen0 = r.Metrics.Gen0Collections,
                        Gen1 = r.Metrics.Gen1Collections,
                        Gen2 = r.Metrics.Gen2Collections,
                        Status = analysis.Findings
                            .FirstOrDefault(f => f.Method == r.MethodUnderTest)?.Severity ?? "ok"
                    }).ToList()
                    : analysis.Findings.Select(f => new MethodSummaryRow
                    {
                        Method = f.Method,
                        Mean = f.Evidence.GetValueOrDefault("mean", "N/A"),
                        Median = f.Evidence.GetValueOrDefault("mean", "N/A"),
                        Allocated = f.Evidence.GetValueOrDefault("allocated", "N/A"),
                        Gen0 = int.TryParse(f.Evidence.GetValueOrDefault("gen0"), out var g0) ? g0 : 0,
                        Gen1 = 0,
                        Gen2 = 0,
                        Status = f.Severity
                    }).ToList()
            },
            Findings = analysis.Findings
        };
}
