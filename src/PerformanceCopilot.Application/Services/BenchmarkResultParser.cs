using System.Globalization;
using System.Text.Json;
using PerformanceCopilot.Benchmarking.Models;

namespace PerformanceCopilot.Application.Services;

public sealed class BenchmarkResultParser
{
    public List<BenchmarkMethodResult> ParseFromRawOutput(string rawOutputPath, string benchmarkRunId)
    {
        if (!File.Exists(rawOutputPath)) return [];

        var lines = File.ReadAllLines(rawOutputPath);

        var fullNameMap = BuildFullNameMap(lines);

        return ParseSummaryTables(lines, fullNameMap);
    }

    private static Dictionary<string, string> BuildFullNameMap(string[] lines)
    {
        var map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        foreach (var line in lines)
        {
            var trimmed = line.Trim();
            if (trimmed.StartsWith("//")) continue;
            var colonIdx = trimmed.IndexOf(": ShortRun(", StringComparison.Ordinal);
            if (colonIdx <= 0) continue;

            var fullName = trimmed[..colonIdx];
            var dotIdx = fullName.LastIndexOf('.');
            if (dotIdx < 0) continue;

            var shortName = fullName[(dotIdx + 1)..];
            map.TryAdd(shortName, fullName);
        }

        return map;
    }

    private static List<BenchmarkMethodResult> ParseSummaryTables(
        string[] lines, Dictionary<string, string> fullNameMap)
    {
        var results = new List<BenchmarkMethodResult>();

        for (int i = 0; i < lines.Length; i++)
        {
            var trimmed = lines[i].Trim();

            if (!trimmed.StartsWith("| Method", StringComparison.OrdinalIgnoreCase)
                || !trimmed.Contains("Mean", StringComparison.OrdinalIgnoreCase))
                continue;

            var headers = SplitTableRow(trimmed);
            if (headers is null || headers.Length < 3) continue;

            var colIndex = BuildColumnIndex(headers);

            i += 2;

            while (i < lines.Length)
            {
                var dataLine = lines[i].Trim();
                if (!dataLine.StartsWith('|') || dataLine.StartsWith("|--"))
                    break;

                var cells = SplitTableRow(dataLine);
                if (cells is not null && cells.Length >= 3)
                {
                    var result = ParseRow(cells, colIndex, fullNameMap);
                    if (result is not null) results.Add(result);
                }

                i++;
            }
        }

        return results;
    }

    private static BenchmarkMethodResult? ParseRow(
        string[] cells, Dictionary<string, int> colIndex, Dictionary<string, string> fullNameMap)
    {
        if (!colIndex.TryGetValue("method", out var methodCol) || methodCol >= cells.Length)
            return null;

        var methodName = cells[methodCol].Trim();
        if (string.IsNullOrEmpty(methodName) || methodName.StartsWith("---"))
            return null;

        double mean = colIndex.TryGetValue("mean", out var meanCol) && meanCol < cells.Length
            ? ParseTimeToNs(cells[meanCol]) : 0;

        int gen0 = colIndex.TryGetValue("gen0", out var gen0Col) && gen0Col < cells.Length
            ? ParseGenCount(cells[gen0Col]) : 0;
        int gen1 = colIndex.TryGetValue("gen1", out var gen1Col) && gen1Col < cells.Length
            ? ParseGenCount(cells[gen1Col]) : 0;
        int gen2 = colIndex.TryGetValue("gen2", out var gen2Col) && gen2Col < cells.Length
            ? ParseGenCount(cells[gen2Col]) : 0;

        long allocated = colIndex.TryGetValue("allocated", out var allocCol) && allocCol < cells.Length
            ? ParseBytesToLong(cells[allocCol]) : 0;

        var fullName = fullNameMap.TryGetValue(methodName, out var fn) ? fn : methodName;
        var dotIdx = fullName.LastIndexOf('.');
        var className = dotIdx >= 0 ? fullName[..dotIdx] : "unknown";
        var shortMethod = dotIdx >= 0 ? fullName[(dotIdx + 1)..] : fullName;

        return new BenchmarkMethodResult
        {
            CandidateId = "unknown",
            BenchmarkClassName = className,
            BenchmarkMethodName = shortMethod,
            MethodUnderTest = fullName,
            Metrics = new BenchmarkMetric
            {
                MeanNs = mean,
                MeanReadable = FormatNs(mean),
                MedianNs = mean,
                MedianReadable = FormatNs(mean),
                MinNs = mean,
                MinReadable = FormatNs(mean),
                MaxNs = mean,
                MaxReadable = FormatNs(mean),
                StandardDeviationNs = 0,
                OperationsPerSecond = mean > 0 ? 1_000_000_000.0 / mean : 0,
                AllocatedBytes = allocated,
                AllocatedReadable = FormatBytes(allocated),
                Gen0Collections = gen0,
                Gen1Collections = gen1,
                Gen2Collections = gen2
            }
        };
    }

    private static string[]? SplitTableRow(string line)
    {
        if (!line.StartsWith('|')) return null;
        return line.Split('|', StringSplitOptions.RemoveEmptyEntries)
                   .Select(p => p.Trim())
                   .ToArray();
    }

    private static Dictionary<string, int> BuildColumnIndex(string[] headers)
    {
        var map = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        for (int i = 0; i < headers.Length; i++)
            map.TryAdd(headers[i].Trim().ToLowerInvariant(), i);
        return map;
    }

    private static double ParseTimeToNs(string cell)
    {
        var parts = cell.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length < 2) return 0;
        if (!double.TryParse(parts[0], NumberStyles.Float, CultureInfo.InvariantCulture, out var val))
            return 0;
        return parts[1] switch
        {
            "ns"       => val,
            "us" or "μs" => val * 1_000,
            "ms"       => val * 1_000_000,
            "s"        => val * 1_000_000_000,
            _          => val
        };
    }

    private static int ParseGenCount(string cell)
    {
        var trimmed = cell.Trim();
        if (trimmed == "-" || string.IsNullOrEmpty(trimmed)) return 0;
        if (!double.TryParse(trimmed, NumberStyles.Float, CultureInfo.InvariantCulture, out var val))
            return 0;
        return (int)Math.Round(val);
    }

    private static long ParseBytesToLong(string cell)
    {
        var parts = cell.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length < 2) return 0;
        if (!double.TryParse(parts[0], NumberStyles.Float, CultureInfo.InvariantCulture, out var val))
            return 0;
        return parts[1] switch
        {
            "B"  => (long)val,
            "KB" => (long)(val * 1024),
            "MB" => (long)(val * 1024 * 1024),
            "GB" => (long)(val * 1024L * 1024 * 1024),
            _    => (long)val
        };
    }

    public List<BenchmarkMethodResult> ParseFromFile(string jsonFilePath, string benchmarkRunId)
    {
        if (!File.Exists(jsonFilePath))
            return [];

        var json = File.ReadAllText(jsonFilePath);
        using var doc = JsonDocument.Parse(json);

        if (!doc.RootElement.TryGetProperty("Benchmarks", out var benchmarks))
            return [];

        var results = new List<BenchmarkMethodResult>();

        foreach (var benchmark in benchmarks.EnumerateArray())
        {
            var methodName = benchmark.GetProperty("FullName").GetString() ?? "unknown";
            var statistics = benchmark.GetProperty("Statistics");

            double mean = statistics.TryGetProperty("Mean", out var m) ? m.GetDouble() : 0;
            double median = statistics.TryGetProperty("Median", out var med) ? med.GetDouble() : 0;
            double min = statistics.TryGetProperty("Min", out var mn) ? mn.GetDouble() : 0;
            double max = statistics.TryGetProperty("Max", out var mx) ? mx.GetDouble() : 0;
            double stdDev = statistics.TryGetProperty("StandardDeviation", out var sd) ? sd.GetDouble() : 0;

            long allocated = 0;
            int gen0 = 0, gen1 = 0, gen2 = 0;

            if (benchmark.TryGetProperty("Memory", out var memory))
            {
                allocated = memory.TryGetProperty("BytesAllocatedPerOperation", out var ba) ? ba.GetInt64() : 0;
                gen0 = memory.TryGetProperty("Gen0Collections", out var g0) ? g0.GetInt32() : 0;
                gen1 = memory.TryGetProperty("Gen1Collections", out var g1) ? g1.GetInt32() : 0;
                gen2 = memory.TryGetProperty("Gen2Collections", out var g2) ? g2.GetInt32() : 0;
            }

            results.Add(new BenchmarkMethodResult
            {
                CandidateId = "unknown",
                BenchmarkClassName = ExtractClassName(methodName),
                BenchmarkMethodName = ExtractMethodName(methodName),
                MethodUnderTest = methodName,
                Metrics = new BenchmarkMetric
                {
                    MeanNs = mean,
                    MeanReadable = FormatNs(mean),
                    MedianNs = median,
                    MedianReadable = FormatNs(median),
                    MinNs = min,
                    MinReadable = FormatNs(min),
                    MaxNs = max,
                    MaxReadable = FormatNs(max),
                    StandardDeviationNs = stdDev,
                    OperationsPerSecond = mean > 0 ? 1_000_000_000 / mean : 0,
                    AllocatedBytes = allocated,
                    AllocatedReadable = FormatBytes(allocated),
                    Gen0Collections = gen0,
                    Gen1Collections = gen1,
                    Gen2Collections = gen2
                }
            });
        }

        return results;
    }

    private static string FormatNs(double ns)
    {
        if (ns >= 1_000_000_000) return $"{ns / 1_000_000_000:F3} s";
        if (ns >= 1_000_000) return $"{ns / 1_000_000:F3} ms";
        if (ns >= 1_000) return $"{ns / 1_000:F3} μs";
        return $"{ns:F3} ns";
    }

    private static string FormatBytes(long bytes)
    {
        if (bytes >= 1024 * 1024) return $"{bytes / (1024.0 * 1024):F2} MB";
        if (bytes >= 1024) return $"{bytes / 1024.0:F2} KB";
        return $"{bytes} B";
    }

    private static string ExtractClassName(string fullName)
    {
        var parts = fullName.Split('.');
        return parts.Length >= 2 ? parts[^2] : fullName;
    }

    private static string ExtractMethodName(string fullName)
    {
        var parts = fullName.Split('.');
        return parts.Length >= 1 ? parts[^1] : fullName;
    }
}
