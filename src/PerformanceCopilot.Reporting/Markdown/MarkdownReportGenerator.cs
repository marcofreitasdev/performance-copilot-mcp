using PerformanceCopilot.Reporting.Models;

namespace PerformanceCopilot.Reporting.Markdown;

public sealed class MarkdownReportGenerator
{
    public string Generate(PerformanceReport report)
    {
        var sb = new System.Text.StringBuilder();

        sb.AppendLine("# Performance Benchmark Report");
        sb.AppendLine();

        sb.AppendLine("## Environment");
        sb.AppendLine();
        sb.AppendLine($"- Runtime: {report.Environment.TargetFramework}");
        sb.AppendLine($"- Configuration: {report.Environment.Configuration}");
        sb.AppendLine($"- OS: {report.Environment.Os}");
        sb.AppendLine($"- Architecture: {report.Environment.Architecture}");
        sb.AppendLine($"- Logical cores: {report.Environment.LogicalCores}");
        sb.AppendLine();

        sb.AppendLine("## Summary");
        sb.AppendLine();
        sb.AppendLine("| Method | Mean | Median | Allocated | Gen0 | Gen1 | Gen2 | Status |");
        sb.AppendLine("|---|---:|---:|---:|---:|---:|---:|---|");

        foreach (var row in report.Summary.Rows)
        {
            sb.AppendLine(
                $"| {row.Method} | {row.Mean} | {row.Median} | {row.Allocated} | " +
                $"{row.Gen0} | {row.Gen1} | {row.Gen2} | {row.Status} |");
        }

        sb.AppendLine();

        sb.AppendLine("## Technical Findings");
        sb.AppendLine();

        foreach (var finding in report.Findings)
        {
            sb.AppendLine($"### {finding.Method}");
            sb.AppendLine();
            sb.AppendLine($"Severity: **{finding.Severity}** | Category: `{finding.Category}`");
            sb.AppendLine();
            sb.AppendLine(finding.Description);
            sb.AppendLine();

            if (finding.Evidence.Count > 0)
            {
                sb.AppendLine("Measured data:");
                sb.AppendLine();
                foreach (var (key, value) in finding.Evidence)
                    sb.AppendLine($"- {key}: {value}");
                sb.AppendLine();
            }

            if (finding.PossibleCauses.Count > 0)
            {
                sb.AppendLine("Detected causes:");
                sb.AppendLine();
                foreach (var cause in finding.PossibleCauses)
                    sb.AppendLine($"- {cause}");
                sb.AppendLine();
            }

            if (finding.OptimizationSuggestions.Count > 0)
            {
                sb.AppendLine("Recommended actions:");
                sb.AppendLine();
                foreach (var suggestion in finding.OptimizationSuggestions)
                    sb.AppendLine($"- {suggestion}");
                sb.AppendLine();
            }
        }

        return sb.ToString();
    }
}
