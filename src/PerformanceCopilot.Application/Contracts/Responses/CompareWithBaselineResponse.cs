using PerformanceCopilot.Application.Services;

namespace PerformanceCopilot.Application.Contracts.Responses;

public sealed class CompareWithBaselineResponse
{
    public required string ComparisonId { get; init; }
    public required string CurrentRunId { get; init; }
    public required string BaselineRunId { get; init; }
    public required string Status { get; init; }
    public List<MethodComparisonResult> Results { get; init; } = [];
}
