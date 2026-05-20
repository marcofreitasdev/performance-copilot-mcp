namespace PerformanceCopilot.Application.Contracts.Requests;

public sealed class CompareWithBaselineRequest
{
    public required string CurrentRunId { get; init; }
    public string? BaselineRunId { get; init; }
    public string? BaselineName { get; init; }
    public double RegressionThresholdPercent { get; init; } = 10;
    public double MemoryRegressionThresholdPercent { get; init; } = 15;
}
