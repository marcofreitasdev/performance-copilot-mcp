using ModelContextProtocol.Server;
using System.ComponentModel;
using System.Text.Json;
using PerformanceCopilot.Application.Contracts.Requests;
using PerformanceCopilot.Application.UseCases;
using PerformanceCopilot.McpServer.Configuration;

namespace PerformanceCopilot.McpServer.Tools;

[McpServerToolType]
public static class CompareWithBaselineTool
{
    [McpServerTool(Name = "compare_with_baseline")]
    [Description("Compares a benchmark run against a saved baseline to detect performance regressions or improvements.")]
    public static async Task<string> ExecuteAsync(
        CompareWithBaselineUseCase useCase,
        [Description("The benchmarkRunId of the current (new) execution")] string currentRunId,
        [Description("The benchmarkRunId of the baseline (optional if baselineName is provided)")] string? baselineRunId = null,
        [Description("Named baseline saved previously (optional if baselineRunId is provided)")] string? baselineName = null,
        [Description("Time regression threshold in percent (default 10)")] double regressionThresholdPercent = 10,
        [Description("Memory regression threshold in percent (default 15)")] double memoryRegressionThresholdPercent = 15,
        CancellationToken ct = default)
    {
        var request = new CompareWithBaselineRequest
        {
            CurrentRunId = currentRunId,
            BaselineRunId = baselineRunId,
            BaselineName = baselineName,
            RegressionThresholdPercent = regressionThresholdPercent,
            MemoryRegressionThresholdPercent = memoryRegressionThresholdPercent
        };

        var response = await useCase.ExecuteAsync(request, ct);
        return JsonSerializer.Serialize(response, JsonOptions.Default);
    }
}
