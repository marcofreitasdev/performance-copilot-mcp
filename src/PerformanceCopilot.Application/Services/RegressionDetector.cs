using PerformanceCopilot.Benchmarking.Models;

namespace PerformanceCopilot.Application.Services;

public sealed class RegressionDetector
{
    public MethodComparisonResult Compare(
        BenchmarkMethodResult baseline,
        BenchmarkMethodResult current,
        double timeThresholdPercent,
        double memoryThresholdPercent)
    {
        var timeChange = CalculateChangePercent(baseline.Metrics.MeanNs, current.Metrics.MeanNs);
        var memoryChange = CalculateChangePercent(baseline.Metrics.AllocatedBytes, current.Metrics.AllocatedBytes);

        var status = DetermineStatus(timeChange, memoryChange, timeThresholdPercent, memoryThresholdPercent);
        var severity = DetermineSeverity(timeChange, memoryChange, timeThresholdPercent, memoryThresholdPercent);

        return new MethodComparisonResult
        {
            Method = current.MethodUnderTest,
            Baseline = new MetricSnapshot(baseline.Metrics.MeanNs, baseline.Metrics.AllocatedBytes),
            Current = new MetricSnapshot(current.Metrics.MeanNs, current.Metrics.AllocatedBytes),
            Diff = new MetricDiff(
                TimeChangePercent: Math.Round(timeChange, 2),
                MemoryChangePercent: Math.Round(memoryChange, 2),
                Status: status,
                Severity: severity
            )
        };
    }

    private static double CalculateChangePercent(double baseline, double current)
    {
        if (baseline == 0) return 0;
        return ((current - baseline) / baseline) * 100;
    }

    private static string DetermineStatus(
        double timeChange, double memoryChange,
        double timeThreshold, double memoryThreshold)
    {
        if (timeChange > timeThreshold || memoryChange > memoryThreshold)
            return "regression";

        if (timeChange < -timeThreshold || memoryChange < -memoryThreshold)
            return "improved";

        return "stable";
    }

    private static string DetermineSeverity(
        double timeChange, double memoryChange,
        double timeThreshold, double memoryThreshold)
    {
        var maxChange = Math.Max(Math.Abs(timeChange), Math.Abs(memoryChange));

        if (maxChange > timeThreshold * 3) return "high";
        if (maxChange > timeThreshold) return "medium";
        return "low";
    }
}

public sealed class MethodComparisonResult
{
    public required string Method { get; init; }
    public required MetricSnapshot Baseline { get; init; }
    public required MetricSnapshot Current { get; init; }
    public required MetricDiff Diff { get; init; }
}

public sealed record MetricSnapshot(double MeanNs, long AllocatedBytes);
public sealed record MetricDiff(
    double TimeChangePercent,
    double MemoryChangePercent,
    string Status,
    string Severity
);
