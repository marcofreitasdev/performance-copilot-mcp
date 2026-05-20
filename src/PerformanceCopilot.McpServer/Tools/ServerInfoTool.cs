using ModelContextProtocol.Server;
using System.ComponentModel;
using System.Text.Json;
using PerformanceCopilot.McpServer.Configuration;

namespace PerformanceCopilot.McpServer.Tools;

[McpServerToolType]
public static class ServerInfoTool
{
    [McpServerTool(Name = "server_info")]
    [Description("Returns Performance Copilot MCP server status, active phase, workspace path and available tools.")]
    public static string GetServerInfo(PerformanceCopilotOptions options)
    {
        return JsonSerializer.Serialize(new
        {
            status = "ok",
            name = "performance-copilot",
            version = "0.1.0",
            activePhase = "phase-1",
            workspacePath = options.AllowedWorkspacePath,
            artifactsBasePath = options.ArtifactsBasePath,
            benchmarkTimeoutSeconds = options.BenchmarkTimeoutSeconds
        });
    }
}
