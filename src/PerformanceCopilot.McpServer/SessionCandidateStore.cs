using PerformanceCopilot.Application.Contracts.Responses;
using System.Collections.Concurrent;

namespace PerformanceCopilot.McpServer;

public sealed class SessionCandidateStore
{
    private readonly ConcurrentDictionary<string, IReadOnlyList<BenchmarkCandidate>> _store = new();

    public void SaveCandidates(string analysisRunId, IReadOnlyList<BenchmarkCandidate> candidates)
    {
        _store[analysisRunId] = candidates;
    }

    public IReadOnlyList<BenchmarkCandidate> GetCandidates(string analysisRunId)
    {
        if (_store.TryGetValue(analysisRunId, out var candidates))
            return candidates;

        throw new InvalidOperationException(
            $"No candidates found for analysisRunId '{analysisRunId}'. " +
            "Run discover_benchmark_candidates first.");
    }

    public BenchmarkCandidate GetCandidate(string candidateId)
    {
        foreach (var candidates in _store.Values)
        {
            var match = candidates.FirstOrDefault(c => c.CandidateId == candidateId);
            if (match is not null) return match;
        }

        throw new InvalidOperationException(
            $"Candidate '{candidateId}' not found. " +
            "Run discover_benchmark_candidates first.");
    }
}
