namespace PerformanceCopilot.Analysis.Models;

public sealed class AnalyzedClass
{
    public required string ClassName { get; init; }
    public required string Namespace { get; init; }
    public required string FilePath { get; init; }
    public List<AnalyzedMethod> Methods { get; init; } = [];
}
