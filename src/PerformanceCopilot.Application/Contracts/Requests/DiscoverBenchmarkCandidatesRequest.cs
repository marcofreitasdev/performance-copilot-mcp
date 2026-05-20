namespace PerformanceCopilot.Application.Contracts.Requests;

public sealed class DiscoverBenchmarkCandidatesRequest
{
    public required string SolutionPath { get; init; }
    public string AnalysisDepth { get; init; } = "standard";
    public bool IncludeTests { get; init; } = false;
    public bool IncludeControllers { get; init; } = true;
    public bool IncludeRepositories { get; init; } = true;
    public bool IncludeServices { get; init; } = true;
    public bool IncludePrivateMethods { get; init; } = false;
    public int MaxCandidates { get; init; } = 20;
    public string[] ExcludedPaths { get; init; } = ["bin", "obj", "Migrations", "Generated"];
}
