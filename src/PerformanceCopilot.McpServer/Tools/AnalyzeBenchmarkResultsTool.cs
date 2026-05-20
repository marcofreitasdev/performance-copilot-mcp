using ModelContextProtocol.Server;
using System.ComponentModel;
using System.Text.Json;
using PerformanceCopilot.Application.Contracts.Requests;
using PerformanceCopilot.Application.UseCases;
using PerformanceCopilot.McpServer.Configuration;
using PerformanceCopilot.Storage.Local;

namespace PerformanceCopilot.McpServer.Tools;

[McpServerToolType]
public static class AnalyzeBenchmarkResultsTool
{
    [McpServerTool(Name = "analyze_benchmark_results")]
    [Description("Interprets benchmark execution results and returns technical findings with optimization suggestions.")]
    public static async Task<string> ExecuteAsync(
        AnalyzeBenchmarkResultsUseCase useCase,
        BenchmarkHistoryStore historyStore,
        [Description("The benchmarkRunId from run_benchmark")] string benchmarkRunId,
        CancellationToken ct = default)
    {
        var executionResult = await historyStore.GetAsync(benchmarkRunId, ct)
            ?? throw new InvalidOperationException($"Benchmark run '{benchmarkRunId}' not found.");

        var request = new AnalyzeBenchmarkResultsRequest { BenchmarkRunId = benchmarkRunId };
        var response = useCase.Execute(request, executionResult);
        return JsonSerializer.Serialize(response, JsonOptions.Default);
    }
}
