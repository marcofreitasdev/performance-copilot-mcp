namespace PerformanceCopilot.Application.Contracts.Responses;

public sealed class DiscoverBenchmarkCandidatesResponse
{
    public required string AnalysisRunId { get; init; }
    public required SolutionSummary Solution { get; init; }
    public required CandidateSummary Summary { get; init; }
    public List<BenchmarkCandidate> Candidates { get; init; } = [];
}

public sealed class SolutionSummary
{
    public required string Path { get; init; }
    public int ProjectsFound { get; init; }
    public int ProjectsAnalyzed { get; init; }
    public int ClassesAnalyzed { get; init; }
    public int MethodsAnalyzed { get; init; }
}

public sealed class CandidateSummary
{
    public int BenchmarkCandidates { get; init; }
    public int HighPriorityCandidates { get; init; }
    public int MediumPriorityCandidates { get; init; }
    public int LowPriorityCandidates { get; init; }
}

public sealed class BenchmarkCandidate
{
    public required string CandidateId { get; init; }
    public required string MethodName { get; init; }
    public required string ClassName { get; init; }
    public required string Namespace { get; init; }
    public required string FilePath { get; init; }
    public int LineStart { get; init; }
    public int LineEnd { get; init; }
    public required string ReturnType { get; init; }
    public List<ParameterInfo> Parameters { get; init; } = [];
    public int Score { get; init; }
    public required string Priority { get; init; }
    public required string BenchmarkViability { get; init; }
    public required string EstimatedImpact { get; init; }
    public required string SuggestedBenchmarkType { get; init; }
    public required TechnicalClassification TechnicalClassification { get; init; }
    public required DetectedPatternsSummary DetectedPatterns { get; init; }
    public List<string> Reasons { get; init; } = [];
    public List<string> Risks { get; init; } = [];
}

public sealed record ParameterInfo(string Name, string Type);

public sealed class TechnicalClassification
{
    public required string EstimatedComplexity { get; init; }
    public bool IsDeterministic { get; init; }
    public bool HasExternalDependency { get; init; }
    public bool RequiresFixture { get; init; }
    public required string BenchmarkReliability { get; init; }
}

public sealed class DetectedPatternsSummary
{
    public int Loops { get; init; }
    public int LinqCalls { get; init; }
    public int ObjectAllocations { get; init; }
    public int StringOperations { get; init; }
    public int AsyncCalls { get; init; }
    public int EfCoreCalls { get; init; }
    public int SerializationCalls { get; init; }
}
