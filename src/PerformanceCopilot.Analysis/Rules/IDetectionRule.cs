using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis;
using PerformanceCopilot.Analysis.Models;

namespace PerformanceCopilot.Analysis.Rules;

public interface IDetectionRule
{
    string PatternType { get; }
    DetectedPattern? Detect(MethodDeclarationSyntax method, SemanticModel semanticModel);
}
