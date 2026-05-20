using FluentAssertions;
using PerformanceCopilot.Analysis.Models;
using PerformanceCopilot.Application.Services;

namespace PerformanceCopilot.Analysis.Tests;

public class CandidateScoringServiceTests
{
    [Fact]
    public void Calculate_WithHighValueMethod_ReturnsHighScore()
    {
        var service = new CandidateScoringService();
        var method = new AnalyzedMethod
        {
            MethodName = "ParseRows",
            ClassName = "SpreadsheetParser",
            Namespace = "MyApp",
            FilePath = "/src/MyApp/SpreadsheetParser.cs",
            ReturnType = "IList<string>",
            IsPublic = true,
            LineCount = 30,
            DetectedPatterns =
            [
                new DetectedPattern("loops", 2),
                new DetectedPattern("linq", 3),
                new DetectedPattern("allocations", 4),
            ]
        };

        var score = service.Calculate(method);

        score.Should().BeGreaterThanOrEqualTo(75);
    }

    [Fact]
    public void Calculate_WithTrivialGetter_ReturnsLowScore()
    {
        var service = new CandidateScoringService();
        var method = new AnalyzedMethod
        {
            MethodName = "GetId",
            ClassName = "Entity",
            Namespace = "MyApp",
            FilePath = "/src/MyApp/Entity.cs",
            ReturnType = "int",
            IsPublic = true,
            LineCount = 2,
            DetectedPatterns = []
        };

        var score = service.Calculate(method);

        score.Should().BeLessThan(40);
    }

    [Fact]
    public void Calculate_ScoreIsClamped_Between0And100()
    {
        var service = new CandidateScoringService();
        var method = new AnalyzedMethod
        {
            MethodName = "Process",
            ClassName = "DataProcessor",
            Namespace = "MyApp",
            FilePath = "/src/MyApp/DataProcessor.cs",
            ReturnType = "void",
            IsPublic = true,
            LineCount = 50,
            IsAsync = true,
            DetectedPatterns =
            [
                new DetectedPattern("loops", 5),
                new DetectedPattern("linq", 5),
                new DetectedPattern("allocations", 5),
                new DetectedPattern("string_operations", 3),
                new DetectedPattern("serialization", 2),
                new DetectedPattern("efcore", 2),
            ]
        };

        var score = service.Calculate(method);

        score.Should().BeInRange(0, 100);
    }

    [Fact]
    public void Calculate_WithExternalDependency_AppliesPenalty()
    {
        var service = new CandidateScoringService();
        var method = new AnalyzedMethod
        {
            MethodName = "FetchData",
            ClassName = "DataService",
            Namespace = "MyApp",
            FilePath = "/src/MyApp/DataService.cs",
            ReturnType = "Task<string>",
            IsPublic = true,
            LineCount = 10,
            IsAsync = true,
            DetectedPatterns =
            [
                new DetectedPattern("external_dependency", 1),
            ]
        };

        var withExternal = service.Calculate(method);

        var methodWithoutExternal = new AnalyzedMethod
        {
            MethodName = "FetchData",
            ClassName = "DataService",
            Namespace = "MyApp",
            FilePath = "/src/MyApp/DataService.cs",
            ReturnType = "Task<string>",
            IsPublic = true,
            LineCount = 10,
            IsAsync = true,
            DetectedPatterns = []
        };

        var withoutExternal = service.Calculate(methodWithoutExternal);

        withExternal.Should().BeLessThan(withoutExternal);
    }
}
