namespace PerformanceCopilot.Analysis.Models;

public sealed class AnalyzedSolution
{
    public required string SolutionPath { get; init; }
    public List<AnalyzedProject> Projects { get; init; } = [];
    public int TotalClassesAnalyzed => Projects.Sum(p => p.Classes.Count);
    public int TotalMethodsAnalyzed => Projects.Sum(p => p.Classes.Sum(c => c.Methods.Count));
}
