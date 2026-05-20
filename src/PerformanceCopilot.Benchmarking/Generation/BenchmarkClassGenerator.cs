using PerformanceCopilot.Benchmarking.Models;

namespace PerformanceCopilot.Benchmarking.Generation;

public sealed class BenchmarkClassGenerator
{
    private readonly BenchmarkFixtureGenerator _fixtureGenerator;

    public BenchmarkClassGenerator(BenchmarkFixtureGenerator fixtureGenerator)
    {
        _fixtureGenerator = fixtureGenerator;
    }

    public GeneratedBenchmarkClass Generate(
        BenchmarkTarget target,
        CandidateGenerationInfo candidate)
    {
        var fixture = _fixtureGenerator.GenerateSetupCode(candidate.Parameters);
        var callArgs = string.Join(", ", fixture.CallArguments);

        var code = $$"""
            using BenchmarkDotNet.Attributes;
            using BenchmarkDotNet.Jobs;
            using {{candidate.Namespace}};

            namespace Benchmarks;

            [{{target.RecommendedJob}}Job]
            {{(target.Diagnosers.Contains("MemoryDiagnoser") ? "[MemoryDiagnoser]" : "")}}
            public class {{target.BenchmarkClassName}}
            {
            {{string.Join(Environment.NewLine, fixture.FieldDeclarations.Select(f => "    " + f))}}

                private {{candidate.ClassName}} _sut;

                [GlobalSetup]
                public void Setup()
                {
                    _sut = new {{candidate.ClassName}}();
            {{string.Join(Environment.NewLine, fixture.SetupLines.Select(l => "        " + l))}}
                }

                [Benchmark]
                public {{candidate.ReturnType}} {{target.BenchmarkMethodName}}()
                {
                    return _sut.{{candidate.MethodName}}({{callArgs}});
                }
            }
            """;

        return new GeneratedBenchmarkClass(
            ClassName: target.BenchmarkClassName,
            Code: code,
            RequiresManualReview: fixture.RequiresManualReview,
            ManualReviewReasons: fixture.ManualReviewReasons
        );
    }
}

public sealed record GeneratedBenchmarkClass(
    string ClassName,
    string Code,
    bool RequiresManualReview,
    List<string> ManualReviewReasons
);
