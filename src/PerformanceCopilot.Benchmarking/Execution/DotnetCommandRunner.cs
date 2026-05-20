using System.Diagnostics;
using System.Text;

namespace PerformanceCopilot.Benchmarking.Execution;

public sealed class DotnetCommandRunner
{
    private static readonly HashSet<string> AllowedSubCommands =
        new(StringComparer.OrdinalIgnoreCase) { "build", "run" };

    public async Task<CommandResult> RunAsync(
        string workingDirectory,
        string subCommand,
        string arguments,
        int timeoutSeconds,
        CancellationToken ct = default)
    {
        if (!AllowedSubCommands.Contains(subCommand))
            throw new InvalidOperationException(
                $"Subcommand '{subCommand}' is not allowed. Allowed: {string.Join(", ", AllowedSubCommands)}.");

        using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        cts.CancelAfter(TimeSpan.FromSeconds(timeoutSeconds));

        var psi = new ProcessStartInfo("dotnet", $"{subCommand} {arguments}")
        {
            WorkingDirectory = workingDirectory,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        var stdoutBuilder = new StringBuilder();
        var stderrBuilder = new StringBuilder();

        using var process = new Process { StartInfo = psi };

        process.OutputDataReceived += (_, e) => { if (e.Data is not null) stdoutBuilder.AppendLine(e.Data); };
        process.ErrorDataReceived += (_, e) => { if (e.Data is not null) stderrBuilder.AppendLine(e.Data); };

        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();

        try
        {
            await process.WaitForExitAsync(cts.Token);
            return new CommandResult(process.ExitCode, stdoutBuilder.ToString(), stderrBuilder.ToString(), TimedOut: false);
        }
        catch (OperationCanceledException) when (!ct.IsCancellationRequested)
        {
            process.Kill(entireProcessTree: true);
            return new CommandResult(ExitCode: -1, Stdout: stdoutBuilder.ToString(),
                Stderr: stderrBuilder.ToString(), TimedOut: true);
        }
    }
}

public sealed record CommandResult(int ExitCode, string Stdout, string Stderr, bool TimedOut);
