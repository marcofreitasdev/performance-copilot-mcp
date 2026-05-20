using PerformanceCopilot.Application.Services;

namespace PerformanceCopilot.Application.Tests;

public sealed class BenchmarkResultParserTests
{
    [Fact]
    public void ParseFromRawOutputReadsBenchmarkDotNetSummaryRows()
    {
        var path = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}-raw-output.txt");
        File.WriteAllText(path, """
            BenchmarkClass.FastMethod: ShortRun(IterationCount=3)

            | Method     | Mean     | Gen0   | Allocated |
            |----------- |---------:|-------:|----------:|
            | FastMethod | 10.00 us | 1.0000 |     128 B |
            """);

        try
        {
            var results = new BenchmarkResultParser().ParseFromRawOutput(path, "run_1");

            var result = Assert.Single(results);
            Assert.Equal("BenchmarkClass.FastMethod", result.MethodUnderTest);
            Assert.Equal(10_000, result.Metrics.MeanNs);
            Assert.Equal(128, result.Metrics.AllocatedBytes);
            Assert.Equal(1, result.Metrics.Gen0Collections);
        }
        finally
        {
            File.Delete(path);
        }
    }
}
