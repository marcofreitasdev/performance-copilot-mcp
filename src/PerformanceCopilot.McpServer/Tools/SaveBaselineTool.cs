using ModelContextProtocol.Server;
using System.ComponentModel;
using System.Text.Json;
using PerformanceCopilot.McpServer.Configuration;
using PerformanceCopilot.Storage.Local;

namespace PerformanceCopilot.McpServer.Tools;

[McpServerToolType]
public static class SaveBaselineTool
{
    [McpServerTool(Name = "save_baseline")]
    [Description("Saves a benchmark run as a named baseline for future comparisons.")]
    public static async Task<string> ExecuteAsync(
        BaselineStore baselineStore,
        [Description("The benchmarkRunId to save as baseline")] string benchmarkRunId,
        [Description("Name for this baseline, e.g. 'main', 'before-optimization'")] string baselineName,
        [Description("Optional description")] string? description = null,
        CancellationToken ct = default)
    {
        await baselineStore.SaveBaselineAsync(baselineName, benchmarkRunId, description, ct);
        return JsonSerializer.Serialize(new { saved = true, baselineName, benchmarkRunId }, JsonOptions.Default);
    }
}
