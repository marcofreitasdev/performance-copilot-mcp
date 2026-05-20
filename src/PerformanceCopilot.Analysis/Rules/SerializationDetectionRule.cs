using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using PerformanceCopilot.Analysis.Models;

namespace PerformanceCopilot.Analysis.Rules;

public sealed class SerializationDetectionRule : IDetectionRule
{
    public string PatternType => "serialization";

    private static readonly HashSet<string> SerializationIdentifiers = new(StringComparer.OrdinalIgnoreCase)
    {
        "JsonSerializer", "JsonConvert", "XmlSerializer", "DataContractSerializer",
        "BinaryFormatter", "MessagePackSerializer", "ProtoBuf", "Newtonsoft",
        "JsonDocument", "JsonElement", "Utf8JsonWriter", "Utf8JsonReader"
    };

    public DetectedPattern? Detect(MethodDeclarationSyntax method, SemanticModel _)
    {
        var count = method.DescendantNodes()
            .OfType<IdentifierNameSyntax>()
            .Count(id => SerializationIdentifiers.Contains(id.Identifier.Text));

        return count > 0 ? new DetectedPattern(PatternType, count) : null;
    }
}
