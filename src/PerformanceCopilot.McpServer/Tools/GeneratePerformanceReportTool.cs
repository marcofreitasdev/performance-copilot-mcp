using ModelContextProtocol.Server;
using System.ComponentModel;
using System.Text.Json;
using PerformanceCopilot.Application.Contracts.Requests;
using PerformanceCopilot.Application.UseCases;
using PerformanceCopilot.McpServer.Configuration;
using PerformanceCopilot.Storage.Local;

namespace PerformanceCopilot.McpServer.Tools;

[McpServerToolType]
public static class GeneratePerformanceReportTool
{
    [McpServerTool(Name = "generate_performance_report")]
    [Description("Generates a Markdown performance report with environment, metrics, findings and recommendations.")]
    public static async Task<string> ExecuteAsync(
        GeneratePerformanceReportUseCase useCase,
        AnalyzeBenchmarkResultsUseCase analysisUseCase,
        BenchmarkHistoryStore historyStore,
        [Description("The benchmarkRunId from run_benchmark")] string benchmarkRunId,
        CancellationToken ct = default)
    {
        var executionResult = await historyStore.GetAsync(benchmarkRunId, ct)
            ?? throw new InvalidOperationException($"Run '{benchmarkRunId}' not found.");

        var analysisResponse = analysisUseCase.Execute(
            new AnalyzeBenchmarkResultsRequest { BenchmarkRunId = benchmarkRunId },
            executionResult);

        var request = new GeneratePerformanceReportRequest { BenchmarkRunId = benchmarkRunId };
        var response = await useCase.ExecuteAsync(request, executionResult, analysisResponse, ct);
        return JsonSerializer.Serialize(response, JsonOptions.Default);
    }
}
