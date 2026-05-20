using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using PerformanceCopilot.Analysis.Models;

namespace PerformanceCopilot.Analysis.Rules;

public sealed class LinqDetectionRule : IDetectionRule
{
    public string PatternType => "linq";

    private static readonly HashSet<string> LinqMethods = new(StringComparer.OrdinalIgnoreCase)
    {
        "Where", "Select", "SelectMany", "ToList", "ToArray", "ToDictionary",
        "ToHashSet", "GroupBy", "OrderBy", "OrderByDescending", "ThenBy",
        "ThenByDescending", "First", "FirstOrDefault", "Single", "SingleOrDefault",
        "Any", "All", "Count", "Sum", "Min", "Max", "Average", "Aggregate",
        "Distinct", "Union", "Intersect", "Except", "Skip", "Take", "Concat",
        "Zip", "Join", "GroupJoin", "Flatten", "AsEnumerable", "AsQueryable"
    };

    public DetectedPattern? Detect(MethodDeclarationSyntax method, SemanticModel _)
    {
        var count = method.DescendantNodes()
            .OfType<InvocationExpressionSyntax>()
            .Select(inv => inv.Expression)
            .OfType<MemberAccessExpressionSyntax>()
            .Count(ma => LinqMethods.Contains(ma.Name.Identifier.Text));

        return count > 0 ? new DetectedPattern(PatternType, count) : null;
    }
}
