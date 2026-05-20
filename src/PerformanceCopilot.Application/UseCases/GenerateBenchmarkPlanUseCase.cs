using PerformanceCopilot.Application.Contracts.Requests;
using PerformanceCopilot.Application.Contracts.Responses;
using PerformanceCopilot.Application.Services;
using PerformanceCopilot.Benchmarking.Models;

namespace PerformanceCopilot.Application.UseCases;

public sealed class GenerateBenchmarkPlanUseCase
{
    private readonly BenchmarkPlanService _planService;

    public GenerateBenchmarkPlanUseCase(BenchmarkPlanService planService)
    {
        _planService = planService;
    }

    public (GenerateBenchmarkPlanResponse Response, BenchmarkPlan Plan) Execute(
        GenerateBenchmarkPlanRequest request,
        IReadOnlyList<BenchmarkCandidate> allCandidates)
    {
        var selectedCandidates = allCandidates
            .Where(c => request.CandidateIds.Contains(c.CandidateId))
            .ToList();

        if (selectedCandidates.Count == 0)
            throw new InvalidOperationException("None of the specified candidate IDs were found in the analysis result.");

        var guidPart = Guid.NewGuid().ToString("N")[..6];
        var planId = $"plan_{DateTime.UtcNow:yyyy_MM_dd}_{guidPart}";

        var targets = selectedCandidates
            .Select(c => _planService.BuildTarget(c, request))
            .ToList();

        var topTwo = selectedCandidates.Take(2).Select(c => c.CandidateId).ToList();

        var plan = new BenchmarkPlan
        {
            BenchmarkPlanId = planId,
            AnalysisRunId = request.AnalysisRunId,
            Targets = targets,
            Recommendation = new PlanRecommendation
            {
                StartWith = topTwo,
                Reason = "Os candidatos possuem alto impacto estimado e boa viabilidade de isolamento."
            }
        };

        var response = new GenerateBenchmarkPlanResponse
        {
            BenchmarkPlanId = planId,
            AnalysisRunId = request.AnalysisRunId,
            Targets = targets.Select(MapToDto).ToList(),
            Recommendation = new PlanRecommendationDto
            {
                StartWith = topTwo,
                Reason = "Os candidatos possuem alto impacto estimado e boa viabilidade de isolamento."
            }
        };

        return (response, plan);
    }

    private static BenchmarkTargetDto MapToDto(BenchmarkTarget t) =>
        new()
        {
            CandidateId = t.CandidateId,
            BenchmarkClassName = t.BenchmarkClassName,
            BenchmarkMethodName = t.BenchmarkMethodName,
            BenchmarkType = t.BenchmarkType,
            RequiredFixture = t.RequiredFixture,
            FixtureStrategy = t.FixtureStrategy,
            EstimatedDifficulty = t.EstimatedDifficulty,
            RecommendedJob = t.RecommendedJob,
            Diagnosers = t.Diagnosers,
            Exporters = t.Exporters
        };
}
