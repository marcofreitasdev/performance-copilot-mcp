namespace PerformanceCopilot.Benchmarking.Generation;

public sealed record CandidateGenerationInfo(
    string Namespace,
    string ClassName,
    string MethodName,
    string ReturnType,
    IReadOnlyList<MethodParameter> Parameters
);

public sealed record MethodParameter(string Name, string Type);
