using PerformanceCopilot.Analysis.Models;
using PerformanceCopilot.Application.Contracts.Responses;

namespace PerformanceCopilot.Application.Services;

public sealed class BenchmarkCandidateMapper
{
    public BenchmarkCandidate Map(AnalyzedMethod method, int score, int index)
    {
        var candidateId = $"candidate_{index:D3}";
        var priority = ClassifyPriority(score);
        var benchmarkType = SuggestBenchmarkType(method);
        var hasExternal = method.DetectedPatterns.Any(p => p.PatternType == "external_dependency");

        return new BenchmarkCandidate
        {
            CandidateId = candidateId,
            MethodName = method.MethodName,
            ClassName = method.ClassName,
            Namespace = method.Namespace,
            FilePath = method.FilePath,
            LineStart = method.LineStart,
            LineEnd = method.LineEnd,
            ReturnType = method.ReturnType,
            Parameters = method.Parameters
                .Select(p => new ParameterInfo(p.Name, p.Type))
                .ToList(),
            Score = score,
            Priority = priority,
            BenchmarkViability = hasExternal ? "low" : "high",
            EstimatedImpact = priority,
            SuggestedBenchmarkType = benchmarkType,
            TechnicalClassification = BuildClassification(method, hasExternal),
            DetectedPatterns = BuildPatternsSummary(method),
            Reasons = BuildReasons(method),
            Risks = BuildRisks(method, hasExternal)
        };
    }

    private static string ClassifyPriority(int score) => score switch
    {
        >= 75 => "high",
        >= 40 => "medium",
        _ => "low"
    };

    private static string SuggestBenchmarkType(AnalyzedMethod method)
    {
        var efcore = method.DetectedPatterns.Any(p => p.PatternType == "efcore");
        var serialization = method.DetectedPatterns.Any(p => p.PatternType == "serialization");
        var allocations = method.DetectedPatterns.Any(p => p.PatternType == "allocations");

        if (efcore) return "ef_query";
        if (serialization) return "serialization";
        if (allocations) return "allocation";
        return "cpu_memory";
    }

    private static TechnicalClassification BuildClassification(AnalyzedMethod method, bool hasExternal) =>
        new()
        {
            EstimatedComplexity = method.DetectedPatterns.Any(p => p.PatternType == "loops") ? "O(n)" : "O(1)",
            IsDeterministic = !method.IsAsync && !hasExternal,
            HasExternalDependency = hasExternal,
            RequiresFixture = method.Parameters.Count > 0,
            BenchmarkReliability = hasExternal ? "low" : "high"
        };

    private static DetectedPatternsSummary BuildPatternsSummary(AnalyzedMethod method) =>
        new()
        {
            Loops = GetCount(method, "loops"),
            LinqCalls = GetCount(method, "linq"),
            ObjectAllocations = GetCount(method, "allocations"),
            StringOperations = GetCount(method, "string_operations"),
            AsyncCalls = method.IsAsync ? 1 : 0,
            EfCoreCalls = GetCount(method, "efcore"),
            SerializationCalls = GetCount(method, "serialization")
        };

    private static List<string> BuildReasons(AnalyzedMethod method)
    {
        var reasons = new List<string>();
        if (method.IsPublic) reasons.Add("Public method accessible without reflection");
        if (method.DetectedPatterns.Any(p => p.PatternType == "loops")) reasons.Add("Contains loops");
        if (method.DetectedPatterns.Any(p => p.PatternType == "linq")) reasons.Add("Uses LINQ");
        if (method.DetectedPatterns.Any(p => p.PatternType == "allocations")) reasons.Add("Performs allocations");
        if (method.LineCount > 20) reasons.Add("Method with extensive logic");
        return reasons;
    }

    private static List<string> BuildRisks(AnalyzedMethod method, bool hasExternal)
    {
        var risks = new List<string>();
        if (hasExternal) risks.Add("External dependency may compromise reproducibility");
        if (method.Parameters.Count > 0) risks.Add("Requires representative input data generation");
        if (method.IsAsync) risks.Add("Async method may require additional context setup");
        return risks;
    }

    private static int GetCount(AnalyzedMethod method, string type) =>
        method.DetectedPatterns.FirstOrDefault(p => p.PatternType == type)?.Count ?? 0;
}
