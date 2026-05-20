using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using PerformanceCopilot.Analysis.Models;

namespace PerformanceCopilot.Analysis.Roslyn;

public sealed class ProjectAnalyzer
{
    private readonly MethodAnalyzer _methodAnalyzer;
    private readonly string[] _excludedPaths;

    public ProjectAnalyzer(MethodAnalyzer methodAnalyzer, string[] excludedPaths)
    {
        _methodAnalyzer = methodAnalyzer;
        _excludedPaths = excludedPaths;
    }

    public async Task<AnalyzedProject> AnalyzeAsync(Project project, CancellationToken ct = default)
    {
        var classes = new List<AnalyzedClass>();

        foreach (var document in project.Documents)
        {
            if (IsExcluded(document.FilePath))
                continue;

            var syntaxRoot = await document.GetSyntaxRootAsync(ct);
            var semanticModel = await document.GetSemanticModelAsync(ct);

            if (syntaxRoot is null || semanticModel is null)
                continue;

            var classDeclarations = syntaxRoot.DescendantNodes().OfType<ClassDeclarationSyntax>();

            foreach (var classDecl in classDeclarations)
            {
                var methods = classDecl.Members
                    .OfType<MethodDeclarationSyntax>()
                    .Where(m => IsPublicOrInternal(m))
                    .Select(m => _methodAnalyzer.Analyze(m, semanticModel, document.FilePath ?? string.Empty))
                    .ToList();

                if (methods.Count == 0) continue;

                var classSymbol = semanticModel.GetDeclaredSymbol(classDecl);

                classes.Add(new AnalyzedClass
                {
                    ClassName = classSymbol?.Name ?? classDecl.Identifier.Text,
                    Namespace = classSymbol?.ContainingNamespace.ToDisplayString() ?? string.Empty,
                    FilePath = document.FilePath ?? string.Empty,
                    Methods = methods
                });
            }
        }

        return new AnalyzedProject
        {
            ProjectName = project.Name,
            ProjectPath = project.FilePath ?? string.Empty,
            LoadedSuccessfully = true,
            Classes = classes
        };
    }

    private bool IsExcluded(string? filePath)
    {
        if (filePath is null) return true;
        return _excludedPaths.Any(e =>
            filePath.Contains(Path.DirectorySeparatorChar + e + Path.DirectorySeparatorChar,
                StringComparison.OrdinalIgnoreCase));
    }

    private static bool IsPublicOrInternal(MethodDeclarationSyntax method)
    {
        return method.Modifiers.Any(m =>
            m.IsKind(SyntaxKind.PublicKeyword) ||
            m.IsKind(SyntaxKind.InternalKeyword));
    }
}
