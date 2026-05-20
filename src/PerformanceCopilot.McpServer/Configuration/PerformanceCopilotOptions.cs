namespace PerformanceCopilot.McpServer.Configuration;

public sealed class PerformanceCopilotOptions
{
    public string AllowedWorkspacePath { get; init; } = string.Empty;
    public string ArtifactsBasePath { get; init; } = ".performance-copilot";
    public int BenchmarkTimeoutSeconds { get; init; } = 600;
    public int MaxFileSizeBytes { get; init; } = 10 * 1024 * 1024;
    public string[] ExcludedPaths { get; init; } = ["bin", "obj", ".git", "node_modules", "Migrations", "Generated"];
}
