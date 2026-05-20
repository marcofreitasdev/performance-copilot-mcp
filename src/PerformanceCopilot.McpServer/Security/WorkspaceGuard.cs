namespace PerformanceCopilot.McpServer.Security;

public sealed class WorkspaceGuard
{
    private readonly string _allowedRoot;

    public WorkspaceGuard(string allowedWorkspacePath)
    {
        if (string.IsNullOrWhiteSpace(allowedWorkspacePath))
            throw new ArgumentException("Workspace path must be provided.", nameof(allowedWorkspacePath));

        _allowedRoot = Path.GetFullPath(allowedWorkspacePath);
    }

    public void EnsurePathIsAllowed(string path)
    {
        var fullPath = Path.GetFullPath(path);

        if (!fullPath.StartsWith(_allowedRoot, StringComparison.OrdinalIgnoreCase))
            throw new UnauthorizedAccessException(
                $"Access denied. Path '{fullPath}' is outside the allowed workspace '{_allowedRoot}'.");
    }

    public bool IsPathAllowed(string path)
    {
        try
        {
            EnsurePathIsAllowed(path);
            return true;
        }
        catch (UnauthorizedAccessException)
        {
            return false;
        }
    }
}
