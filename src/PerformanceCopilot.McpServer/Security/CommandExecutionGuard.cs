namespace PerformanceCopilot.McpServer.Security;

public sealed class CommandExecutionGuard
{
    private static readonly HashSet<string> AllowedExecutables = new(StringComparer.OrdinalIgnoreCase)
    {
        "dotnet"
    };

    private static readonly HashSet<string> AllowedSubCommands = new(StringComparer.OrdinalIgnoreCase)
    {
        "build", "restore", "run", "new", "add", "sln"
    };

    public void EnsureCommandIsAllowed(string executable, string subCommand)
    {
        if (!AllowedExecutables.Contains(executable))
            throw new InvalidOperationException(
                $"Execution of '{executable}' is not allowed. Only 'dotnet' is permitted.");

        if (!AllowedSubCommands.Contains(subCommand))
            throw new InvalidOperationException(
                $"Subcommand '{subCommand}' is not allowed. Allowed: {string.Join(", ", AllowedSubCommands)}.");
    }
}
