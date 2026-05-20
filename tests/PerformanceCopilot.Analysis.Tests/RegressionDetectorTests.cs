using FluentAssertions;
using PerformanceCopilot.Application.Services;
using PerformanceCopilot.Benchmarking.Models;

namespace PerformanceCopilot.Analysis.Tests;

public class RegressionDetectorTests
{
    private readonly RegressionDetector _detector = new();

    [Fact]
    public void Compare_WhenTimeIncreasedAboveThreshold_ReturnsRegression()
    {
        var baseline = MakeResult(meanNs: 1_000_000, allocatedBytes: 100_000);
        var current = MakeResult(meanNs: 1_200_000, allocatedBytes: 100_000);

        var result = _detector.Compare(baseline, current, timeThresholdPercent: 10, memoryThresholdPercent: 15);

        result.Diff.Status.Should().Be("regression");
        result.Diff.TimeChangePercent.Should().BeApproximately(20, 0.1);
    }

    [Fact]
    public void Compare_WhenTimeDecreasedAboveThreshold_ReturnsImproved()
    {
        var baseline = MakeResult(meanNs: 1_000_000, allocatedBytes: 100_000);
        var current = MakeResult(meanNs: 800_000, allocatedBytes: 100_000);

        var result = _detector.Compare(baseline, current, timeThresholdPercent: 10, memoryThresholdPercent: 15);

        result.Diff.Status.Should().Be("improved");
        result.Diff.TimeChangePercent.Should().BeApproximately(-20, 0.1);
    }

    [Fact]
    public void Compare_WhenWithinThresholds_ReturnsStable()
    {
        var baseline = MakeResult(meanNs: 1_000_000, allocatedBytes: 100_000);
        var current = MakeResult(meanNs: 1_050_000, allocatedBytes: 102_000);

        var result = _detector.Compare(baseline, current, timeThresholdPercent: 10, memoryThresholdPercent: 15);

        result.Diff.Status.Should().Be("stable");
    }

    [Fact]
    public void Compare_WhenMemoryIncreasedAboveThreshold_ReturnsRegression()
    {
        var baseline = MakeResult(meanNs: 1_000_000, allocatedBytes: 100_000);
        var current = MakeResult(meanNs: 1_000_000, allocatedBytes: 120_000);

        var result = _detector.Compare(baseline, current, timeThresholdPercent: 10, memoryThresholdPercent: 15);

        result.Diff.Status.Should().Be("regression");
        result.Diff.MemoryChangePercent.Should().BeApproximately(20, 0.1);
    }

    [Fact]
    public void Compare_WhenTimeSpikeIsVeryHigh_ReturnsSeverityHigh()
    {
        var baseline = MakeResult(meanNs: 1_000_000, allocatedBytes: 100_000);
        var current = MakeResult(meanNs: 1_400_000, allocatedBytes: 100_000);

        var result = _detector.Compare(baseline, current, timeThresholdPercent: 10, memoryThresholdPercent: 15);

        result.Diff.Severity.Should().Be("high");
    }

    [Fact]
    public void SanitizeName_RemovesInvalidCharacters()
    {
        var sanitized = PerformanceCopilot.Storage.Local.BaselineStore.SanitizeName("../evil/../path");

        sanitized.Should().NotContain("..");
        sanitized.Should().NotContain("/");
    }

    private static BenchmarkMethodResult MakeResult(double meanNs, long allocatedBytes) =>
        new()
        {
            CandidateId = "candidate_001",
            BenchmarkClassName = "TestBenchmark",
            BenchmarkMethodName = "Test_Benchmark",
            MethodUnderTest = "TestClass.TestMethod",
            Metrics = new BenchmarkMetric
            {
                MeanNs = meanNs,
                MeanReadable = $"{meanNs} ns",
                MedianNs = meanNs,
                MedianReadable = $"{meanNs} ns",
                MinNs = meanNs,
                MinReadable = $"{meanNs} ns",
                MaxNs = meanNs,
                MaxReadable = $"{meanNs} ns",
                AllocatedBytes = allocatedBytes,
                AllocatedReadable = $"{allocatedBytes} B",
                OperationsPerSecond = meanNs > 0 ? 1_000_000_000 / meanNs : 0
            }
        };
}
