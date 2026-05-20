using PerformanceCopilot.Application.Contracts.Requests;
using PerformanceCopilot.Application.Contracts.Responses;
using PerformanceCopilot.Benchmarking.Generation;
using PerformanceCopilot.Benchmarking.Models;

namespace PerformanceCopilot.Application.UseCases;

public sealed class GenerateBenchmarkForCandidateUseCase
{
    private readonly BenchmarkClassGenerator _classGenerator;

    public GenerateBenchmarkForCandidateUseCase(BenchmarkClassGenerator classGenerator)
    {
        _classGenerator = classGenerator;
    }

    public async Task<GenerateBenchmarkForCandidateResponse> ExecuteAsync(
        GenerateBenchmarkForCandidateRequest request,
        BenchmarkTarget target,
        BenchmarkCandidate candidate,
        CancellationToken ct = default)
    {
        var candidateInfo = new CandidateGenerationInfo(
            Namespace: candidate.Namespace,
            ClassName: candidate.ClassName,
            MethodName: candidate.MethodName,
            ReturnType: candidate.ReturnType,
            Parameters: candidate.Parameters.Select(p => new MethodParameter(p.Name, p.Type)).ToList()
        );

        var generated = _classGenerator.Generate(target, candidateInfo);

        var fileName = $"{generated.ClassName}.cs";
        var filePath = Path.Combine(request.BenchmarkProjectPath, fileName);

        if (File.Exists(filePath) && !request.OverwriteExisting)
            return new GenerateBenchmarkForCandidateResponse
            {
                Generated = false,
                CandidateId = request.CandidateId,
                BenchmarkFilePath = filePath,
                BenchmarkClassName = generated.ClassName,
                BenchmarkMethodName = target.BenchmarkMethodName,
                RequiresManualReview = generated.RequiresManualReview,
                ManualReviewReasons = generated.ManualReviewReasons,
                SkippedReason = "File already exists. Set overwriteExisting to true to replace it."
            };

        await File.WriteAllTextAsync(filePath, generated.Code, ct);

        return new GenerateBenchmarkForCandidateResponse
        {
            Generated = true,
            CandidateId = request.CandidateId,
            BenchmarkFilePath = filePath,
            BenchmarkClassName = generated.ClassName,
            BenchmarkMethodName = target.BenchmarkMethodName,
            RequiresManualReview = generated.RequiresManualReview,
            ManualReviewReasons = generated.ManualReviewReasons
        };
    }
}
