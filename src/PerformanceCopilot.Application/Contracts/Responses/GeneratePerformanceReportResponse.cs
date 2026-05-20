namespace PerformanceCopilot.Application.Contracts.Responses;

public sealed class GeneratePerformanceReportResponse
{
    public bool Generated { get; init; }
    public required string ReportPath { get; init; }
    public List<string> Sections { get; init; } = [];
}
