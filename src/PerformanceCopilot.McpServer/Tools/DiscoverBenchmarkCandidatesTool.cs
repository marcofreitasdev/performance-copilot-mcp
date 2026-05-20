using ModelContextProtocol.Server;
using System.ComponentModel;
using System.Text.Json;
using PerformanceCopilot.Application.Contracts.Requests;
using PerformanceCopilot.Application.UseCases;
using PerformanceCopilot.McpServer.Configuration;
using PerformanceCopilot.McpServer.Security;

namespace PerformanceCopilot.McpServer.Tools;

[McpServerToolType]
public static class DiscoverBenchmarkCandidatesTool
{
    [McpServerTool(Name = "discover_benchmark_candidates")]
    [Description("Analyzes a .NET solution and returns prioritized benchmark candidate methods with scores, detected patterns, and technical justification.")]
    public static async Task<string> ExecuteAsync(
        DiscoverBenchmarkCandidatesUseCase useCase,
        WorkspaceGuard workspaceGuard,
        SessionCandidateStore candidateStore,
        [Description("Absolute or relative path to the .sln or .slnx file")] string solutionPath,
        [Description("Analysis depth: standard or deep")] string analysisDepth = "standard",
        [Description("Maximum number of candidates to return")] int maxCandidates = 20,
        CancellationToken ct = default)
    {
        workspaceGuard.EnsurePathIsAllowed(solutionPath);

        var request = new DiscoverBenchmarkCandidatesRequest
        {
            SolutionPath = solutionPath,
            AnalysisDepth = analysisDepth,
            MaxCandidates = maxCandidates
        };

        var response = await useCase.ExecuteAsync(request, ct);

        candidateStore.SaveCandidates(response.AnalysisRunId, response.Candidates);

        return JsonSerializer.Serialize(response, JsonOptions.Default);
    }
}
