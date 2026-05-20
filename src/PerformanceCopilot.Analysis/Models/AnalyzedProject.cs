namespace PerformanceCopilot.Analysis.Models;

public sealed class AnalyzedProject
{
    public required string ProjectName { get; init; }
    public required string ProjectPath { get; init; }
    public List<AnalyzedClass> Classes { get; init; } = [];
    public bool LoadedSuccessfully { get; init; }
    public string? LoadError { get; init; }
}
