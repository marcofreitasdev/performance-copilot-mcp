using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using PerformanceCopilot.Analysis.Models;

namespace PerformanceCopilot.Analysis.Rules;

public sealed class ExternalDependencyDetectionRule : IDetectionRule
{
    public string PatternType => "external_dependency";

    private static readonly HashSet<string> ExternalIdentifiers = new(StringComparer.OrdinalIgnoreCase)
    {
        "HttpClient", "IHttpClientFactory", "HttpRequestMessage", "HttpResponseMessage",
        "WebClient", "RestClient", "IRestClient", "GrpcChannel"
    };

    private static readonly HashSet<string> ExternalMethods = new(StringComparer.OrdinalIgnoreCase)
    {
        "GetAsync", "PostAsync", "PutAsync", "DeleteAsync", "PatchAsync",
        "SendAsync", "GetStringAsync", "GetByteArrayAsync", "GetStreamAsync"
    };

    private static readonly HashSet<string> DelayIdentifiers = new(StringComparer.OrdinalIgnoreCase)
    {
        "Delay", "GetEnvironmentVariable", "GetEnvironmentVariables"
    };

    public DetectedPattern? Detect(MethodDeclarationSyntax method, SemanticModel _)
    {
        var identifierCount = method.DescendantNodes()
            .OfType<IdentifierNameSyntax>()
            .Count(id => ExternalIdentifiers.Contains(id.Identifier.Text));

        var methodCount = method.DescendantNodes()
            .OfType<InvocationExpressionSyntax>()
            .Select(inv => inv.Expression)
            .OfType<MemberAccessExpressionSyntax>()
            .Count(ma => ExternalMethods.Contains(ma.Name.Identifier.Text)
                      || DelayIdentifiers.Contains(ma.Name.Identifier.Text));

        var total = identifierCount + methodCount;
        return total > 0 ? new DetectedPattern(PatternType, total) : null;
    }
}
