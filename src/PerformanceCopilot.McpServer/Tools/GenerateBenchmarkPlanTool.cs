using ModelContextProtocol.Server;
using System.ComponentModel;
using System.Text.Json;
using PerformanceCopilot.Application.Contracts.Requests;
using PerformanceCopilot.Application.UseCases;
using PerformanceCopilot.McpServer.Configuration;

namespace PerformanceCopilot.McpServer.Tools;

[McpServerToolType]
public static class GenerateBenchmarkPlanTool
{
    [McpServerTool(Name = "generate_benchmark_plan")]
    [Description("Transforms selected benchmark candidates into a technical benchmark execution plan with job configuration, diagnosers, and fixture strategies.")]
    public static string Execute(
        GenerateBenchmarkPlanUseCase useCase,
        SessionCandidateStore candidateStore,
        SessionPlanStore planStore,
        [Description("The analysisRunId returned by discover_benchmark_candidates")] string analysisRunId,
        [Description("Array of candidateIds to include in the plan")] string[] candidateIds,
        [Description("Benchmark mode: short, default, or medium")] string benchmarkMode = "short",
        [Description("Include MemoryDiagnoser in the plan")] bool includeMemoryDiagnoser = true,
        [Description("Include JSON exporter")] bool includeJsonExporter = true,
        [Description("Include Markdown exporter")] bool includeMarkdownExporter = true)
    {
        var candidates = candidateStore.GetCandidates(analysisRunId);

        var request = new GenerateBenchmarkPlanRequest
        {
            AnalysisRunId = analysisRunId,
            CandidateIds = candidateIds.ToList(),
            BenchmarkMode = benchmarkMode,
            IncludeMemoryDiagnoser = includeMemoryDiagnoser,
            IncludeJsonExporter = includeJsonExporter,
            IncludeMarkdownExporter = includeMarkdownExporter
        };

        var (response, plan) = useCase.Execute(request, candidates);
        planStore.SavePlan(plan);
        return JsonSerializer.Serialize(response, JsonOptions.Default);
    }
}
