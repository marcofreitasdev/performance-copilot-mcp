namespace PerformanceCopilot.Application.Contracts.Requests;

public sealed class GenerateBenchmarkPlanRequest
{
    public required string AnalysisRunId { get; init; }
    public required List<string> CandidateIds { get; init; }
    public string BenchmarkMode { get; init; } = "short";
    public bool IncludeMemoryDiagnoser { get; init; } = true;
    public bool IncludeJsonExporter { get; init; } = true;
    public bool IncludeMarkdownExporter { get; init; } = true;
}
