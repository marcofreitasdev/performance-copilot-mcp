using Microsoft.Build.Locator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.MSBuild;

namespace PerformanceCopilot.Analysis.Roslyn;

public sealed class SolutionLoader
{
    public async Task<Solution> LoadAsync(string solutionPath, CancellationToken ct = default)
    {
        if (!MSBuildLocator.IsRegistered)
            MSBuildLocator.RegisterDefaults();

        var workspace = MSBuildWorkspace.Create();
        workspace.RegisterWorkspaceFailedHandler(args =>
        {
            Console.Error.WriteLine($"[MSBuildWorkspace] {args.Diagnostic.Kind}: {args.Diagnostic.Message}");
        });

        return await workspace.OpenSolutionAsync(solutionPath, cancellationToken: ct);
    }
}
