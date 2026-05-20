using PerformanceCopilot.Reporting.Models;

namespace PerformanceCopilot.Application.Contracts.Responses;

public sealed class AnalyzeBenchmarkResultsResponse
{
    public required string BenchmarkRunId { get; init; }
    public List<TechnicalFinding> Findings { get; init; } = [];
    public required OverallAssessment OverallAssessment { get; init; }
}

public sealed class OverallAssessment
{
    public required string Status { get; init; }
    public required string PrimaryBottleneck { get; init; }
    public required string RecommendedNextAction { get; init; }
}
