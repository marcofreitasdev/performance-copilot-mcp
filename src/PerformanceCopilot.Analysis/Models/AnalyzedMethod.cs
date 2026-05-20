namespace PerformanceCopilot.Analysis.Models;

public sealed class AnalyzedMethod
{
    public required string MethodName { get; init; }
    public required string ClassName { get; init; }
    public required string Namespace { get; init; }
    public required string FilePath { get; init; }
    public int LineStart { get; init; }
    public int LineEnd { get; init; }
    public required string ReturnType { get; init; }
    public List<MethodParameter> Parameters { get; init; } = [];
    public bool IsPublic { get; init; }
    public bool IsInternal { get; init; }
    public bool IsAsync { get; init; }
    public bool IsStatic { get; init; }
    public int LineCount { get; init; }
    public List<DetectedPattern> DetectedPatterns { get; init; } = [];
}

public sealed record MethodParameter(string Name, string Type);
