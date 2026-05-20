using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using PerformanceCopilot.Analysis.Models;

namespace PerformanceCopilot.Analysis.Rules;

public sealed class StringOperationDetectionRule : IDetectionRule
{
    public string PatternType => "string_operations";

    private static readonly HashSet<string> StringMethods = new(StringComparer.OrdinalIgnoreCase)
    {
        "Format", "Concat", "Join", "Split", "Replace", "Trim", "TrimStart",
        "TrimEnd", "Substring", "IndexOf", "LastIndexOf", "Contains", "StartsWith",
        "EndsWith", "ToUpper", "ToLower", "ToUpperInvariant", "ToLowerInvariant",
        "PadLeft", "PadRight", "Remove", "Insert", "Normalize", "IsNullOrEmpty",
        "IsNullOrWhiteSpace", "Compare", "CompareOrdinal"
    };

    public DetectedPattern? Detect(MethodDeclarationSyntax method, SemanticModel _)
    {
        var methodCallCount = method.DescendantNodes()
            .OfType<InvocationExpressionSyntax>()
            .Select(inv => inv.Expression)
            .OfType<MemberAccessExpressionSyntax>()
            .Count(ma => StringMethods.Contains(ma.Name.Identifier.Text));

        var stringBuilderCount = method.DescendantNodes()
            .OfType<IdentifierNameSyntax>()
            .Count(id => id.Identifier.Text == "StringBuilder");

        var loopNodes = method.DescendantNodes()
            .Where(n => n is ForStatementSyntax or ForEachStatementSyntax
                     or WhileStatementSyntax or DoStatementSyntax)
            .ToList();

        var concatInLoopCount = loopNodes
            .SelectMany(l => l.DescendantNodes().OfType<BinaryExpressionSyntax>())
            .Count(b => b.IsKind(SyntaxKind.AddExpression));

        var total = methodCallCount + stringBuilderCount + concatInLoopCount;
        return total > 0 ? new DetectedPattern(PatternType, total) : null;
    }
}
