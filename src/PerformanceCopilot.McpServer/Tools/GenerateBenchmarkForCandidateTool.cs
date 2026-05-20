using ModelContextProtocol.Server;
using System.ComponentModel;
using System.Text.Json;
using PerformanceCopilot.Application.Contracts.Requests;
using PerformanceCopilot.Application.UseCases;
using PerformanceCopilot.McpServer.Configuration;
using PerformanceCopilot.McpServer.Security;

namespace PerformanceCopilot.McpServer.Tools;

[McpServerToolType]
public static class GenerateBenchmarkForCandidateTool
{
    [McpServerTool(Name = "generate_benchmark_for_candidate")]
    [Description("Generates a BenchmarkDotNet class file for a specific benchmark candidate identified by the plan.")]
    public static async Task<string> ExecuteAsync(
        GenerateBenchmarkForCandidateUseCase useCase,
        SessionPlanStore planStore,
        SessionCandidateStore candidateStore,
        WorkspaceGuard workspaceGuard,
        [Description("The benchmarkPlanId returned by generate_benchmark_plan")] string benchmarkPlanId,
        [Description("The candidateId to generate a benchmark for")] string candidateId,
        [Description("Absolute path to the benchmark project directory")] string benchmarkProjectPath,
        [Description("Overwrite the file if it already exists")] bool overwriteExisting = false,
        CancellationToken ct = default)
    {
        workspaceGuard.EnsurePathIsAllowed(benchmarkProjectPath);

        var target = planStore.GetTarget(benchmarkPlanId, candidateId);
        var candidate = candidateStore.GetCandidate(candidateId);

        var request = new GenerateBenchmarkForCandidateRequest
        {
            BenchmarkPlanId = benchmarkPlanId,
            CandidateId = candidateId,
            BenchmarkProjectPath = benchmarkProjectPath,
            OverwriteExisting = overwriteExisting
        };

        var result = await useCase.ExecuteAsync(request, target, candidate, ct);
        return JsonSerializer.Serialize(result, JsonOptions.Default);
    }
}
