namespace PerformanceCopilot.Storage.Models;

public sealed class BaselineMetadata
{
    public required string BaselineName { get; init; }
    public required string BenchmarkRunId { get; init; }
    public DateTime SavedAt { get; init; }
    public string? Description { get; init; }
}
