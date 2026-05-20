namespace PerformanceCopilot.Application.Contracts.Requests;

public sealed class AnalyzeBenchmarkResultsRequest
{
    public required string BenchmarkRunId { get; init; }
    public bool IncludeOptimizationSuggestions { get; init; } = true;
    public bool IncludeRiskAssessment { get; init; } = true;
}
