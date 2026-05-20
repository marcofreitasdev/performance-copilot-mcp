namespace PerformanceCopilot.Application.Contracts.Requests;

public sealed class GeneratePerformanceReportRequest
{
    public required string BenchmarkRunId { get; init; }
    public bool IncludeEnvironment { get; init; } = true;
    public bool IncludeRawMetrics { get; init; } = true;
    public bool IncludeFindings { get; init; } = true;
    public bool IncludeRecommendations { get; init; } = true;
    public string Format { get; init; } = "markdown";
}
