using PerformanceCopilot.Benchmarking.Models;
using System.Runtime.InteropServices;

namespace PerformanceCopilot.Benchmarking.Execution;

public sealed class BenchmarkDotNetRunner
{
    private readonly DotnetCommandRunner _commandRunner;
    private readonly string _artifactsBasePath;

    public BenchmarkDotNetRunner(DotnetCommandRunner commandRunner, string artifactsBasePath)
    {
        _commandRunner = commandRunner;
        _artifactsBasePath = artifactsBasePath;
    }

    public async Task<BenchmarkExecutionResult> RunAsync(
        RunBenchmarkRequest request,
        CancellationToken ct = default)
    {
        var runId = $"bench_{DateTime.UtcNow:yyyy_MM_dd}_{Guid.NewGuid():N}".Substring(0, 28);
        var runDir = Path.Combine(_artifactsBasePath, "runs", runId);
        Directory.CreateDirectory(runDir);

        var startTime = DateTime.UtcNow;

        var buildResult = await _commandRunner.RunAsync(
            workingDirectory: request.BenchmarkProjectPath,
            subCommand: "build",
            arguments: $"-c {request.Configuration}",
            timeoutSeconds: 120,
            ct);

        if (buildResult.ExitCode != 0)
            return FailedResult(runId, runDir, "Build failed", buildResult.Stderr, startTime, request.Configuration);

        var filterArg = string.IsNullOrWhiteSpace(request.Filter) ? "" : $"--filter \"{request.Filter}\"";
        var runResult = await _commandRunner.RunAsync(
            workingDirectory: request.BenchmarkProjectPath,
            subCommand: "run",
            arguments: $"-c {request.Configuration} -- {filterArg}",
            timeoutSeconds: request.TimeoutSeconds,
            ct);

        var rawOutputPath = Path.Combine(runDir, "raw-output.txt");
        await File.WriteAllTextAsync(rawOutputPath,
            runResult.Stdout + Environment.NewLine + runResult.Stderr, ct);

        if (runResult.TimedOut)
            return FailedResult(runId, runDir, "Benchmark execution timed out", runResult.Stderr, startTime, request.Configuration);

        if (runResult.ExitCode != 0)
            return FailedResult(runId, runDir, "Benchmark execution failed", runResult.Stderr, startTime, request.Configuration);

        var elapsed = (long)(DateTime.UtcNow - startTime).TotalMilliseconds;

        var bdnArtifacts = FindBenchmarkDotNetArtifacts(request.BenchmarkProjectPath);

        return new BenchmarkExecutionResult
        {
            BenchmarkRunId = runId,
            Status = "completed",
            Environment = BuildEnvironmentInfo(request.Configuration),
            Summary = new ExecutionSummary
            {
                TotalBenchmarks = 1,
                SuccessfulBenchmarks = 1,
                FailedBenchmarks = 0,
                TotalExecutionTimeMs = elapsed
            },
            Results = [],
            Artifacts = new ArtifactPaths
            {
                RootPath = runDir,
                SummaryJsonPath = Path.Combine(runDir, "summary.json"),
                TechnicalReportPath = Path.Combine(runDir, "technical-report.md"),
                BenchmarkDotNetJsonPath = bdnArtifacts.JsonPath,
                BenchmarkDotNetMarkdownPath = bdnArtifacts.MarkdownPath
            }
        };
    }

    private static BenchmarkExecutionResult FailedResult(
        string runId, string runDir, string message, string details, DateTime startTime, string configuration) =>
        new()
        {
            BenchmarkRunId = runId,
            Status = "failed",
            Environment = new EnvironmentInfo
            {
                TargetFramework = "unknown",
                Configuration = configuration
            },
            Summary = new ExecutionSummary
            {
                TotalBenchmarks = 0,
                SuccessfulBenchmarks = 0,
                FailedBenchmarks = 1,
                TotalExecutionTimeMs = (long)(DateTime.UtcNow - startTime).TotalMilliseconds
            },
            Artifacts = new ArtifactPaths
            {
                RootPath = runDir,
                SummaryJsonPath = Path.Combine(runDir, "summary.json"),
                TechnicalReportPath = Path.Combine(runDir, "technical-report.md")
            },
            ErrorMessage = $"{message}: {details}"
        };

    private static EnvironmentInfo BuildEnvironmentInfo(string configuration) =>
        new()
        {
            TargetFramework = "net10.0",
            Configuration = configuration,
            Os = RuntimeInformation.OSDescription,
            Architecture = RuntimeInformation.OSArchitecture.ToString(),
            LogicalCores = Environment.ProcessorCount
        };

    private static (string? JsonPath, string? MarkdownPath) FindBenchmarkDotNetArtifacts(string projectPath)
    {
        var resultsDir = Path.Combine(projectPath, "BenchmarkDotNet.Artifacts", "results");
        if (!Directory.Exists(resultsDir)) return (null, null);

        var jsonFile = Directory.GetFiles(resultsDir, "*-report-full.json").FirstOrDefault();
        var mdFile = Directory.GetFiles(resultsDir, "*-report-github.md").FirstOrDefault();

        return (jsonFile, mdFile);
    }
}
