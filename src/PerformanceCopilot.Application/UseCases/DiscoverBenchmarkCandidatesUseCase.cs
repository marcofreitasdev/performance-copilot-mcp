using PerformanceCopilot.Analysis.Models;
using PerformanceCopilot.Analysis.Roslyn;
using PerformanceCopilot.Application.Contracts.Requests;
using PerformanceCopilot.Application.Contracts.Responses;
using PerformanceCopilot.Application.Services;

namespace PerformanceCopilot.Application.UseCases;

public sealed class DiscoverBenchmarkCandidatesUseCase
{
    private readonly SolutionLoader _solutionLoader;
    private readonly ProjectAnalyzer _projectAnalyzer;
    private readonly CandidateScoringService _scoringService;
    private readonly BenchmarkCandidateMapper _mapper;

    public DiscoverBenchmarkCandidatesUseCase(
        SolutionLoader solutionLoader,
        ProjectAnalyzer projectAnalyzer,
        CandidateScoringService scoringService,
        BenchmarkCandidateMapper mapper)
    {
        _solutionLoader = solutionLoader;
        _projectAnalyzer = projectAnalyzer;
        _scoringService = scoringService;
        _mapper = mapper;
    }

    public async Task<DiscoverBenchmarkCandidatesResponse> ExecuteAsync(
        DiscoverBenchmarkCandidatesRequest request,
        CancellationToken ct = default)
    {
        var runId = $"analysis_{DateTime.UtcNow:yyyy_MM_dd}_{Guid.NewGuid():N}".Substring(0, 28);

        var solution = await _solutionLoader.LoadAsync(request.SolutionPath, ct);
        var analyzedProjects = new List<AnalyzedProject>();

        foreach (var project in solution.Projects)
        {
            if (!request.IncludeTests && IsTestProject(project.Name))
                continue;

            var analyzed = await _projectAnalyzer.AnalyzeAsync(project, ct);
            analyzedProjects.Add(analyzed);
        }

        var allMethods = analyzedProjects
            .SelectMany(p => p.Classes)
            .SelectMany(c => c.Methods)
            .ToList();

        var scoredMethods = allMethods
            .Select(m => (Method: m, Score: _scoringService.Calculate(m)))
            .Where(x => x.Score >= 40)
            .OrderByDescending(x => x.Score)
            .Take(request.MaxCandidates)
            .ToList();

        var candidates = scoredMethods
            .Select((x, i) => _mapper.Map(x.Method, x.Score, i + 1))
            .ToList();

        return new DiscoverBenchmarkCandidatesResponse
        {
            AnalysisRunId = runId,
            Solution = new SolutionSummary
            {
                Path = request.SolutionPath,
                ProjectsFound = solution.Projects.Count(),
                ProjectsAnalyzed = analyzedProjects.Count,
                ClassesAnalyzed = analyzedProjects.Sum(p => p.Classes.Count),
                MethodsAnalyzed = allMethods.Count
            },
            Summary = new CandidateSummary
            {
                BenchmarkCandidates = candidates.Count,
                HighPriorityCandidates = candidates.Count(c => c.Priority == "high"),
                MediumPriorityCandidates = candidates.Count(c => c.Priority == "medium"),
                LowPriorityCandidates = candidates.Count(c => c.Priority == "low")
            },
            Candidates = candidates
        };
    }

    private static bool IsTestProject(string projectName) =>
        projectName.Contains("Test", StringComparison.OrdinalIgnoreCase) ||
        projectName.Contains("Spec", StringComparison.OrdinalIgnoreCase);
}
