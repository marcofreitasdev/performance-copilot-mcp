namespace PerformanceCopilot.Benchmarking.Execution;

public sealed class RunBenchmarkRequest
{
    public required string BenchmarkProjectPath { get; init; }
    public string Configuration { get; init; } = "Release";
    public string? Filter { get; init; }
    public int TimeoutSeconds { get; init; } = 600;
    public bool SaveArtifacts { get; init; } = true;
}
