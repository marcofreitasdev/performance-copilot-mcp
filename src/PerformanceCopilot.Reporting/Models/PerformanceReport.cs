using PerformanceCopilot.Benchmarking.Models;

namespace PerformanceCopilot.Reporting.Models;

public sealed class PerformanceReport
{
    public required string BenchmarkRunId { get; init; }
    public required EnvironmentInfo Environment { get; init; }
    public required ReportSummary Summary { get; init; }
    public List<TechnicalFinding> Findings { get; init; } = [];
    public List<OptimizationSuggestion> Recommendations { get; init; } = [];
}

public sealed class ReportSummary
{
    public List<MethodSummaryRow> Rows { get; init; } = [];
}

public sealed class MethodSummaryRow
{
    public required string Method { get; init; }
    public required string Mean { get; init; }
    public required string Median { get; init; }
    public required string Allocated { get; init; }
    public int Gen0 { get; init; }
    public int Gen1 { get; init; }
    public int Gen2 { get; init; }
    public int Score { get; init; }
    public required string Status { get; init; }
}

public sealed class TechnicalFinding
{
    public required string Method { get; init; }
    public required string Severity { get; init; }
    public required string Category { get; init; }
    public required string Description { get; init; }
    public Dictionary<string, string> Evidence { get; init; } = [];
    public List<string> PossibleCauses { get; init; } = [];
    public List<string> OptimizationSuggestions { get; init; } = [];
}

public sealed class OptimizationSuggestion
{
    public required string Method { get; init; }
    public required string Suggestion { get; init; }
    public required string Priority { get; init; }
}
