using PerformanceCopilot.Benchmarking.Models;
using System.Collections.Concurrent;

namespace PerformanceCopilot.McpServer;

public sealed class SessionPlanStore
{
    private readonly ConcurrentDictionary<string, BenchmarkPlan> _store = new();

    public void SavePlan(BenchmarkPlan plan)
    {
        _store[plan.BenchmarkPlanId] = plan;
    }

    public BenchmarkTarget GetTarget(string benchmarkPlanId, string candidateId)
    {
        if (!_store.TryGetValue(benchmarkPlanId, out var plan))
            throw new InvalidOperationException(
                $"No plan found for benchmarkPlanId '{benchmarkPlanId}'. " +
                "Run generate_benchmark_plan first.");

        var target = plan.Targets.FirstOrDefault(t => t.CandidateId == candidateId)
            ?? throw new InvalidOperationException(
                $"No target found for candidateId '{candidateId}' in plan '{benchmarkPlanId}'.");

        return target;
    }
}
