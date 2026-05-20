using FluentAssertions;
using Microsoft.CodeAnalysis.CSharp;
using PerformanceCopilot.Analysis.Rules;

namespace PerformanceCopilot.Analysis.Tests;

public class LoopDetectionRuleTests
{
    private readonly LoopDetectionRule _rule = new();

    [Fact]
    public void Detect_MethodWithForLoop_ReturnsPattern()
    {
        var source = """
            class C {
                public void M() {
                    for (int i = 0; i < 10; i++) { }
                }
            }
            """;

        var tree = CSharpSyntaxTree.ParseText(source);
        var compilation = CSharpCompilation.Create("test", [tree],
            [Microsoft.CodeAnalysis.MetadataReference.CreateFromFile(typeof(object).Assembly.Location)]);
        var semanticModel = compilation.GetSemanticModel(tree);
        var method = tree.GetRoot()
            .DescendantNodes()
            .OfType<Microsoft.CodeAnalysis.CSharp.Syntax.MethodDeclarationSyntax>()
            .First();

        var result = _rule.Detect(method, semanticModel);

        result.Should().NotBeNull();
        result!.PatternType.Should().Be("loops");
        result.Count.Should().Be(1);
    }

    [Fact]
    public void Detect_MethodWithMultipleLoops_CountsAll()
    {
        var source = """
            class C {
                public void M() {
                    for (int i = 0; i < 10; i++) { }
                    foreach (var x in new int[0]) { }
                    while (false) { }
                    do { } while (false);
                }
            }
            """;

        var tree = CSharpSyntaxTree.ParseText(source);
        var compilation = CSharpCompilation.Create("test", [tree],
            [Microsoft.CodeAnalysis.MetadataReference.CreateFromFile(typeof(object).Assembly.Location)]);
        var semanticModel = compilation.GetSemanticModel(tree);
        var method = tree.GetRoot()
            .DescendantNodes()
            .OfType<Microsoft.CodeAnalysis.CSharp.Syntax.MethodDeclarationSyntax>()
            .First();

        var result = _rule.Detect(method, semanticModel);

        result.Should().NotBeNull();
        result!.Count.Should().Be(4);
    }

    [Fact]
    public void Detect_MethodWithNoLoops_ReturnsNull()
    {
        var source = """
            class C {
                public int M() { return 42; }
            }
            """;

        var tree = CSharpSyntaxTree.ParseText(source);
        var compilation = CSharpCompilation.Create("test", [tree],
            [Microsoft.CodeAnalysis.MetadataReference.CreateFromFile(typeof(object).Assembly.Location)]);
        var semanticModel = compilation.GetSemanticModel(tree);
        var method = tree.GetRoot()
            .DescendantNodes()
            .OfType<Microsoft.CodeAnalysis.CSharp.Syntax.MethodDeclarationSyntax>()
            .First();

        var result = _rule.Detect(method, semanticModel);

        result.Should().BeNull();
    }
}
