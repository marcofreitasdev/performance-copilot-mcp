using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using PerformanceCopilot.Analysis.Models;

namespace PerformanceCopilot.Analysis.Rules;

public sealed class LoopDetectionRule : IDetectionRule
{
    public string PatternType => "loops";

    public DetectedPattern? Detect(MethodDeclarationSyntax method, SemanticModel _)
    {
        var count = method.DescendantNodes()
            .Count(n => n is ForStatementSyntax
                     or ForEachStatementSyntax
                     or WhileStatementSyntax
                     or DoStatementSyntax);

        return count > 0 ? new DetectedPattern(PatternType, count) : null;
    }
}
