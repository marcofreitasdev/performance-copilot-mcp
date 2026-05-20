namespace PerformanceCopilot.Application.Contracts.Requests;

public sealed class GenerateBenchmarkForCandidateRequest
{
    public required string BenchmarkPlanId { get; init; }
    public required string CandidateId { get; init; }
    public required string BenchmarkProjectPath { get; init; }
    public bool OverwriteExisting { get; init; } = false;
}
