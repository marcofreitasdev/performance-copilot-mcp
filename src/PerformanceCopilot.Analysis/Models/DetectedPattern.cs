namespace PerformanceCopilot.Analysis.Models;

public sealed record DetectedPattern(
    string PatternType,
    int Count,
    string? Detail = null
);
