using PerformanceCopilot.Analysis.Models;

namespace PerformanceCopilot.Application.Services;

public sealed class CandidateScoringService
{
    private static readonly HashSet<string> HighValueClassSuffixes = new(StringComparer.OrdinalIgnoreCase)
    {
        "Service", "Handler", "Processor", "Parser", "Mapper",
        "Repository", "Validator", "Generator"
    };

    public int Calculate(AnalyzedMethod method)
    {
        var score = 0;

        if (method.IsPublic) score += 20;
        else if (method.IsInternal) score += 20;

        if (HighValueClassSuffixes.Any(s => method.ClassName.EndsWith(s, StringComparison.OrdinalIgnoreCase)))
            score += 20;

        var loops = GetPatternCount(method, "loops");
        var linq = GetPatternCount(method, "linq");
        var allocations = GetPatternCount(method, "allocations");
        var strings = GetPatternCount(method, "string_operations");
        var serialization = GetPatternCount(method, "serialization");
        var efcore = GetPatternCount(method, "efcore");
        var external = GetPatternCount(method, "external_dependency");

        if (loops > 0) score += 15;
        if (linq > 0) score += 15;
        if (strings > 0) score += 15;
        if (allocations > 0) score += 15;
        if (method.IsAsync) score += 10;
        if (serialization > 0) score += 10;
        if (efcore > 0) score += 10;
        if (method.LineCount > 20) score += 10;

        if (method.LineCount < 5) score -= 10;
        if (external > 0) score -= 30;
        if (!method.IsPublic && !method.IsInternal) score -= 40;

        return Math.Clamp(score, 0, 100);
    }

    private static int GetPatternCount(AnalyzedMethod method, string patternType) =>
        method.DetectedPatterns.FirstOrDefault(p => p.PatternType == patternType)?.Count ?? 0;
}
