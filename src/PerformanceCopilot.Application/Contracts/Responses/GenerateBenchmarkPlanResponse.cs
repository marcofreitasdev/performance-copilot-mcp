namespace PerformanceCopilot.Application.Contracts.Responses;

public sealed class GenerateBenchmarkPlanResponse
{
    public required string BenchmarkPlanId { get; init; }
    public required string AnalysisRunId { get; init; }
    public List<BenchmarkTargetDto> Targets { get; init; } = [];
    public required PlanRecommendationDto Recommendation { get; init; }
}

public sealed class BenchmarkTargetDto
{
    public required string CandidateId { get; init; }
    public required string BenchmarkClassName { get; init; }
    public required string BenchmarkMethodName { get; init; }
    public required string BenchmarkType { get; init; }
    public bool RequiredFixture { get; init; }
    public required string FixtureStrategy { get; init; }
    public required string EstimatedDifficulty { get; init; }
    public required string RecommendedJob { get; init; }
    public List<string> Diagnosers { get; init; } = [];
    public List<string> Exporters { get; init; } = [];
}

public sealed class PlanRecommendationDto
{
    public List<string> StartWith { get; init; } = [];
    public required string Reason { get; init; }
}
