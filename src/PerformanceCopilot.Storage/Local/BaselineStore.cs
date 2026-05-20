using System.Text.Json;
using PerformanceCopilot.Storage.Models;

namespace PerformanceCopilot.Storage.Local;

public sealed class BaselineStore
{
    private readonly string _basePath;

    public BaselineStore(string basePath)
    {
        _basePath = basePath;
    }

    public async Task SaveBaselineAsync(
        string baselineName,
        string benchmarkRunId,
        string? description = null,
        CancellationToken ct = default)
    {
        var baselinesDir = Path.Combine(_basePath, "baselines");
        Directory.CreateDirectory(baselinesDir);

        var metadata = new BaselineMetadata
        {
            BaselineName = baselineName,
            BenchmarkRunId = benchmarkRunId,
            SavedAt = DateTime.UtcNow,
            Description = description
        };

        var path = Path.Combine(baselinesDir, $"{SanitizeName(baselineName)}.json");

        await File.WriteAllTextAsync(
            path,
            JsonSerializer.Serialize(metadata, new JsonSerializerOptions { WriteIndented = true }),
            ct);
    }

    public async Task<BaselineMetadata?> GetBaselineAsync(string baselineName, CancellationToken ct = default)
    {
        var path = Path.Combine(_basePath, "baselines", $"{SanitizeName(baselineName)}.json");

        if (!File.Exists(path)) return null;

        var json = await File.ReadAllTextAsync(path, ct);
        return JsonSerializer.Deserialize<BaselineMetadata>(json);
    }

    public async Task<List<BaselineMetadata>> ListBaselinesAsync(CancellationToken ct = default)
    {
        var baselinesDir = Path.Combine(_basePath, "baselines");
        if (!Directory.Exists(baselinesDir)) return [];

        var results = new List<BaselineMetadata>();

        foreach (var file in Directory.GetFiles(baselinesDir, "*.json"))
        {
            var json = await File.ReadAllTextAsync(file, ct);
            var entry = JsonSerializer.Deserialize<BaselineMetadata>(json);
            if (entry is not null) results.Add(entry);
        }

        return results.OrderByDescending(b => b.SavedAt).ToList();
    }

    public static string SanitizeName(string name) =>
        string.Concat(name.Where(c => char.IsLetterOrDigit(c) || c == '-' || c == '_'));
}
