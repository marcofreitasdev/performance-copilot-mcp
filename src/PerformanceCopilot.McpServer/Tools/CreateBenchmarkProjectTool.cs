using ModelContextProtocol.Server;
using System.ComponentModel;
using System.Text.Json;
using PerformanceCopilot.Benchmarking.Generation;
using PerformanceCopilot.Benchmarking.Models;
using PerformanceCopilot.McpServer.Configuration;
using PerformanceCopilot.McpServer.Security;

namespace PerformanceCopilot.McpServer.Tools;

[McpServerToolType]
public static class CreateBenchmarkProjectTool
{
    [McpServerTool(Name = "create_benchmark_project")]
    [Description("Creates a BenchmarkDotNet project in the solution, ready to receive generated benchmark classes.")]
    public static async Task<string> ExecuteAsync(
        BenchmarkProjectGenerator generator,
        WorkspaceGuard workspaceGuard,
        [Description("Path to the .sln or .slnx file")] string solutionPath,
        [Description("Name for the benchmark project, e.g. MyApp.Benchmarks")] string benchmarkProjectName,
        [Description("Directory where the benchmark project will be created")] string benchmarkProjectPath,
        [Description("Target framework, default net10.0")] string targetFramework = "net10.0",
        [Description("Relative .csproj paths to reference from the benchmark project")] string[]? referenceProjects = null,
        CancellationToken ct = default)
    {
        workspaceGuard.EnsurePathIsAllowed(solutionPath);
        workspaceGuard.EnsurePathIsAllowed(benchmarkProjectPath);

        var request = new CreateBenchmarkProjectRequest
        {
            SolutionPath = solutionPath,
            BenchmarkProjectName = benchmarkProjectName,
            BenchmarkProjectPath = benchmarkProjectPath,
            TargetFramework = targetFramework,
            AddToSolution = true,
            ReferenceProjects = referenceProjects?.ToList() ?? []
        };

        var result = await generator.CreateAsync(request, ct);
        return JsonSerializer.Serialize(result, JsonOptions.Default);
    }
}
