using System.Text.Json;
using PerformanceCopilot.Benchmarking.Models;

namespace PerformanceCopilot.Storage.Local;

public sealed class BenchmarkHistoryStore
{
    private readonly string _basePath;

    public BenchmarkHistoryStore(string basePath)
    {
        _basePath = basePath;
    }

    public async Task SaveAsync(BenchmarkExecutionResult result, CancellationToken ct = default)
    {
        var runDir = Path.Combine(_basePath, "runs", result.BenchmarkRunId);
        Directory.CreateDirectory(runDir);

        var summaryPath = Path.Combine(runDir, "summary.json");
        var json = JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true });
        await File.WriteAllTextAsync(summaryPath, json, ct);

        await AppendToHistoryIndexAsync(result.BenchmarkRunId, result.Status, ct);
    }

    public async Task<BenchmarkExecutionResult?> GetAsync(string benchmarkRunId, CancellationToken ct = default)
    {
        var summaryPath = Path.Combine(_basePath, "runs", benchmarkRunId, "summary.json");

        if (!File.Exists(summaryPath)) return null;

        var json = await File.ReadAllTextAsync(summaryPath, ct);
        return JsonSerializer.Deserialize<BenchmarkExecutionResult>(json);
    }

    private async Task AppendToHistoryIndexAsync(string runId, string status, CancellationToken ct)
    {
        var historyPath = Path.Combine(_basePath, "history.json");

        var entries = new List<HistoryEntry>();

        if (File.Exists(historyPath))
        {
            var existing = await File.ReadAllTextAsync(historyPath, ct);
            entries = JsonSerializer.Deserialize<List<HistoryEntry>>(existing) ?? [];
        }

        entries.Add(new HistoryEntry(runId, status, DateTime.UtcNow));

        await File.WriteAllTextAsync(
            historyPath,
            JsonSerializer.Serialize(entries, new JsonSerializerOptions { WriteIndented = true }),
            ct);
    }
}

public sealed record HistoryEntry(string RunId, string Status, DateTime ExecutedAt);
