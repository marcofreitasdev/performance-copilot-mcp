using ModelContextProtocol.Server;
using System.ComponentModel;
using System.Text.Json;
using PerformanceCopilot.Benchmarking.Execution;
using PerformanceCopilot.McpServer.Configuration;
using PerformanceCopilot.McpServer.Security;

namespace PerformanceCopilot.McpServer.Tools;

[McpServerToolType]
public static class RunBenchmarkTool
{
    [McpServerTool(Name = "run_benchmark")]
    [Description("Executes BenchmarkDotNet in Release mode and returns performance metrics including time, memory and GC stats.")]
    public static async Task<string> ExecuteAsync(
        BenchmarkDotNetRunner runner,
        WorkspaceGuard workspaceGuard,
        [Description("Absolute path to the benchmark project directory")] string benchmarkProjectPath,
        [Description("Optional BenchmarkDotNet filter, e.g. *MyClass*")] string? filter = null,
        [Description("Timeout in seconds (default 600)")] int timeoutSeconds = 600,
        CancellationToken ct = default)
    {
        workspaceGuard.EnsurePathIsAllowed(benchmarkProjectPath);

        var request = new RunBenchmarkRequest
        {
            BenchmarkProjectPath = benchmarkProjectPath,
            Filter = filter,
            TimeoutSeconds = timeoutSeconds
        };

        var result = await runner.RunAsync(request, ct);
        return JsonSerializer.Serialize(result, JsonOptions.Default);
    }
}
