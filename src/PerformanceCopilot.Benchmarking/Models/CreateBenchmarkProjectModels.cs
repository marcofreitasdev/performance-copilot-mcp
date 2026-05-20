namespace PerformanceCopilot.Benchmarking.Models;

public sealed class CreateBenchmarkProjectRequest
{
    public required string SolutionPath { get; init; }
    public required string BenchmarkProjectName { get; init; }
    public required string BenchmarkProjectPath { get; init; }
    public string TargetFramework { get; init; } = "net10.0";
    public bool AddToSolution { get; init; } = true;
    public List<string> ReferenceProjects { get; init; } = [];
}

public sealed class CreateBenchmarkProjectResult
{
    public bool Created { get; init; }
    public required string BenchmarkProjectPath { get; init; }
    public List<string> FilesCreated { get; init; } = [];
    public List<string> PackagesAdded { get; init; } = [];
    public List<string> ProjectReferencesAdded { get; init; } = [];
}
