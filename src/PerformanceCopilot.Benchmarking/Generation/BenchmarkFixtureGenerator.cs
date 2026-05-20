namespace PerformanceCopilot.Benchmarking.Generation;

public sealed class BenchmarkFixtureGenerator
{
    public FixtureCode GenerateSetupCode(IReadOnlyList<MethodParameter> parameters)
    {
        var fields = new List<string>();
        var setupLines = new List<string>();
        var callArgs = new List<string>();
        var requiresReview = false;
        var reviewReasons = new List<string>();

        foreach (var param in parameters)
        {
            var (fieldDecl, setupLine, argName, needsReview, reason) = GenerateForType(param);
            fields.Add(fieldDecl);
            setupLines.Add(setupLine);
            callArgs.Add(argName);

            if (needsReview)
            {
                requiresReview = true;
                if (reason is not null) reviewReasons.Add(reason);
            }
        }

        return new FixtureCode(fields, setupLines, callArgs, requiresReview, reviewReasons);
    }

    private static (string field, string setup, string arg, bool review, string? reason)
        GenerateForType(MethodParameter param)
    {
        var name = $"_{param.Name}";

        return param.Type.ToLowerInvariant() switch
        {
            "string" or "system.string" =>
                ($"private string {name};",
                 $"{name} = \"sample_data_for_{param.Name}\";",
                 name, false, null),

            "int" or "system.int32" or "long" or "system.int64" =>
                ($"private {param.Type} {name};",
                 $"{name} = 1000;",
                 name, false, null),

            var t when t.StartsWith("ienumerable") || t.StartsWith("ilist") || t.StartsWith("list") =>
                ($"private {param.Type} {name};",
                 $"throw new NotSupportedException(\"Initialize {name} with representative data before running this benchmark.\");",
                 name, true, $"Collection type '{param.Type}' requires representative data initialization"),

            _ =>
                ($"private {param.Type} {name};",
                 $"throw new NotSupportedException(\"Initialize {name} before running this benchmark.\");",
                 name, true, $"Complex type '{param.Type}' requires manual fixture setup")
        };
    }
}

public sealed record FixtureCode(
    List<string> FieldDeclarations,
    List<string> SetupLines,
    List<string> CallArguments,
    bool RequiresManualReview,
    List<string> ManualReviewReasons
);
