using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using PerformanceCopilot.Analysis.Models;

namespace PerformanceCopilot.Analysis.Rules;

public sealed class AllocationDetectionRule : IDetectionRule
{
    public string PatternType => "allocations";

    public DetectedPattern? Detect(MethodDeclarationSyntax method, SemanticModel _)
    {
        var count = method.Body is null && method.ExpressionBody is null
            ? 0
            : method.DescendantNodes()
                .OfType<ObjectCreationExpressionSyntax>()
                .Count();

        count += method.DescendantNodes()
            .OfType<ImplicitObjectCreationExpressionSyntax>()
            .Count();

        count += method.DescendantNodes()
            .OfType<ArrayCreationExpressionSyntax>()
            .Count();

        count += method.DescendantNodes()
            .OfType<ImplicitArrayCreationExpressionSyntax>()
            .Count();

        return count > 0 ? new DetectedPattern(PatternType, count) : null;
    }
}
