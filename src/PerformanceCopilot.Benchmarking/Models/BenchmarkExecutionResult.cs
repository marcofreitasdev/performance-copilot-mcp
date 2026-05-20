namespace PerformanceCopilot.Benchmarking.Models;

public sealed class BenchmarkExecutionResult
{
    public required string BenchmarkRunId { get; init; }
    public required string Status { get; init; }
    public required EnvironmentInfo Environment { get; init; }
    public required ExecutionSummary Summary { get; init; }
    public List<BenchmarkMethodResult> Results { get; init; } = [];
    public required ArtifactPaths Artifacts { get; init; }
    public string? ErrorMessage { get; init; }
}

public sealed class EnvironmentInfo
{
    public required string TargetFramework { get; init; }
    public required string Configuration { get; init; }
    public string Runtime { get; init; } = string.Empty;
    public string Os { get; init; } = string.Empty;
    public string Architecture { get; init; } = string.Empty;
    public int LogicalCores { get; init; }
}

public sealed class ExecutionSummary
{
    public int TotalBenchmarks { get; init; }
    public int SuccessfulBenchmarks { get; init; }
    public int FailedBenchmarks { get; init; }
    public long TotalExecutionTimeMs { get; init; }
}

public sealed class BenchmarkMethodResult
{
    public required string CandidateId { get; init; }
    public required string BenchmarkClassName { get; init; }
    public required string BenchmarkMethodName { get; init; }
    public required string MethodUnderTest { get; init; }
    public required BenchmarkMetric Metrics { get; init; }
}

public sealed class BenchmarkMetric
{
    public double MeanNs { get; init; }
    public required string MeanReadable { get; init; }
    public double MedianNs { get; init; }
    public required string MedianReadable { get; init; }
    public double MinNs { get; init; }
    public required string MinReadable { get; init; }
    public double MaxNs { get; init; }
    public required string MaxReadable { get; init; }
    public double StandardDeviationNs { get; init; }
    public double OperationsPerSecond { get; init; }
    public long AllocatedBytes { get; init; }
    public required string AllocatedReadable { get; init; }
    public int Gen0Collections { get; init; }
    public int Gen1Collections { get; init; }
    public int Gen2Collections { get; init; }
}

public sealed class ArtifactPaths
{
    public required string RootPath { get; init; }
    public required string SummaryJsonPath { get; init; }
    public required string TechnicalReportPath { get; init; }
    public string? BenchmarkDotNetJsonPath { get; init; }
    public string? BenchmarkDotNetMarkdownPath { get; init; }
}
