using PerformanceCopilot.Application.Contracts.Requests;
using PerformanceCopilot.Application.Contracts.Responses;
using PerformanceCopilot.Application.Services;
using PerformanceCopilot.Storage.Local;

namespace PerformanceCopilot.Application.UseCases;

public sealed class CompareWithBaselineUseCase
{
    private readonly BenchmarkHistoryStore _historyStore;
    private readonly BaselineStore _baselineStore;
    private readonly RegressionDetector _detector;

    public CompareWithBaselineUseCase(
        BenchmarkHistoryStore historyStore,
        BaselineStore baselineStore,
        RegressionDetector detector)
    {
        _historyStore = historyStore;
        _baselineStore = baselineStore;
        _detector = detector;
    }

    public async Task<CompareWithBaselineResponse> ExecuteAsync(
        CompareWithBaselineRequest request,
        CancellationToken ct = default)
    {
        var currentRun = await _historyStore.GetAsync(request.CurrentRunId, ct)
            ?? throw new InvalidOperationException($"Run '{request.CurrentRunId}' not found.");

        Benchmarking.Models.BenchmarkExecutionResult baselineRun;

        if (!string.IsNullOrWhiteSpace(request.BaselineRunId))
        {
            baselineRun = await _historyStore.GetAsync(request.BaselineRunId, ct)
                ?? throw new InvalidOperationException($"Baseline run '{request.BaselineRunId}' not found.");
        }
        else if (!string.IsNullOrWhiteSpace(request.BaselineName))
        {
            var metadata = await _baselineStore.GetBaselineAsync(request.BaselineName, ct)
                ?? throw new InvalidOperationException($"Baseline '{request.BaselineName}' not found.");

            baselineRun = await _historyStore.GetAsync(metadata.BenchmarkRunId, ct)
                ?? throw new InvalidOperationException($"Baseline run data for '{request.BaselineName}' not found.");
        }
        else
        {
            throw new ArgumentException("Either baselineRunId or baselineName must be provided.");
        }

        var results = new List<MethodComparisonResult>();

        foreach (var currentMethod in currentRun.Results)
        {
            var baselineMethod = baselineRun.Results
                .FirstOrDefault(b => b.BenchmarkMethodName == currentMethod.BenchmarkMethodName);

            if (baselineMethod is null) continue;

            results.Add(_detector.Compare(
                baselineMethod, currentMethod,
                request.RegressionThresholdPercent,
                request.MemoryRegressionThresholdPercent));
        }

        var overallStatus = results.Any(r => r.Diff.Status == "regression")
            ? "regression_detected"
            : results.Any(r => r.Diff.Status == "improved")
                ? "improvement_detected"
                : "stable";

        return new CompareWithBaselineResponse
        {
            ComparisonId = $"comparison_{DateTime.UtcNow:yyyy_MM_dd}_{Guid.NewGuid().ToString("N")[..6]}",
            CurrentRunId = request.CurrentRunId,
            BaselineRunId = baselineRun.BenchmarkRunId,
            Status = overallStatus,
            Results = results
        };
    }
}
