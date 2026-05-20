namespace PerformanceCopilot.Application.Contracts.Responses;

public sealed class GenerateBenchmarkForCandidateResponse
{
    public bool Generated { get; init; }
    public required string CandidateId { get; init; }
    public required string BenchmarkFilePath { get; init; }
    public required string BenchmarkClassName { get; init; }
    public required string BenchmarkMethodName { get; init; }
    public bool RequiresManualReview { get; init; }
    public List<string> ManualReviewReasons { get; init; } = [];
    public string? SkippedReason { get; init; }
}
