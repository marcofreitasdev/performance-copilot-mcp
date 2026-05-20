using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using PerformanceCopilot.Analysis.Models;
using PerformanceCopilot.Analysis.Rules;

namespace PerformanceCopilot.Analysis.Roslyn;

public sealed class MethodAnalyzer
{
    private readonly IReadOnlyList<IDetectionRule> _rules;

    public MethodAnalyzer(IEnumerable<IDetectionRule> rules)
    {
        _rules = rules.ToList();
    }

    public AnalyzedMethod Analyze(
        MethodDeclarationSyntax method,
        SemanticModel semanticModel,
        string filePath)
    {
        var symbol = semanticModel.GetDeclaredSymbol(method);
        if (symbol is null)
            throw new InvalidOperationException("Could not resolve symbol for method.");

        var lineSpan = method.GetLocation().GetLineSpan();
        var patterns = _rules
            .Select(r => r.Detect(method, semanticModel))
            .Where(p => p is not null)
            .Cast<DetectedPattern>()
            .ToList();

        return new AnalyzedMethod
        {
            MethodName = symbol.Name,
            ClassName = symbol.ContainingType.Name,
            Namespace = symbol.ContainingNamespace.ToDisplayString(),
            FilePath = filePath,
            LineStart = lineSpan.StartLinePosition.Line + 1,
            LineEnd = lineSpan.EndLinePosition.Line + 1,
            ReturnType = symbol.ReturnType.ToDisplayString(),
            Parameters = symbol.Parameters
                .Select(p => new MethodParameter(p.Name, p.Type.ToDisplayString()))
                .ToList(),
            IsPublic = symbol.DeclaredAccessibility == Accessibility.Public,
            IsInternal = symbol.DeclaredAccessibility == Accessibility.Internal,
            IsAsync = symbol.IsAsync,
            IsStatic = symbol.IsStatic,
            LineCount = lineSpan.EndLinePosition.Line - lineSpan.StartLinePosition.Line + 1,
            DetectedPatterns = patterns
        };
    }
}
