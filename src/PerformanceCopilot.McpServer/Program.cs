using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PerformanceCopilot.Analysis.Roslyn;
using PerformanceCopilot.Analysis.Rules;
using PerformanceCopilot.Application.Services;
using PerformanceCopilot.Application.UseCases;
using PerformanceCopilot.Benchmarking.Execution;
using PerformanceCopilot.Benchmarking.Generation;
using PerformanceCopilot.McpServer;
using PerformanceCopilot.McpServer.Configuration;
using PerformanceCopilot.McpServer.Security;
using PerformanceCopilot.Reporting.Markdown;
using PerformanceCopilot.Storage.Local;

var workspacePath = args.Length > 0
    ? Path.GetFullPath(args[0])
    : Directory.GetCurrentDirectory();

var options = new PerformanceCopilotOptions
{
    AllowedWorkspacePath = workspacePath
};

var builder = Host.CreateApplicationBuilder(args);

builder.Logging.AddConsole(consoleLogOptions =>
{
    consoleLogOptions.LogToStandardErrorThreshold = LogLevel.Trace;
});

builder.Services.AddSingleton(options);
builder.Services.AddSingleton(new WorkspaceGuard(options.AllowedWorkspacePath));
builder.Services.AddSingleton<CommandExecutionGuard>();

builder.Services.AddSingleton<SolutionLoader>();
builder.Services.AddSingleton<IDetectionRule, LoopDetectionRule>();
builder.Services.AddSingleton<IDetectionRule, LinqDetectionRule>();
builder.Services.AddSingleton<IDetectionRule, AllocationDetectionRule>();
builder.Services.AddSingleton<IDetectionRule, StringOperationDetectionRule>();
builder.Services.AddSingleton<IDetectionRule, SerializationDetectionRule>();
builder.Services.AddSingleton<IDetectionRule, EfCoreDetectionRule>();
builder.Services.AddSingleton<IDetectionRule, ExternalDependencyDetectionRule>();
builder.Services.AddSingleton<MethodAnalyzer>(sp =>
    new MethodAnalyzer(sp.GetServices<IDetectionRule>()));
builder.Services.AddSingleton<ProjectAnalyzer>(sp =>
    new ProjectAnalyzer(sp.GetRequiredService<MethodAnalyzer>(), options.ExcludedPaths));

builder.Services.AddSingleton<CandidateScoringService>();
builder.Services.AddSingleton<BenchmarkCandidateMapper>();
builder.Services.AddSingleton<DiscoverBenchmarkCandidatesUseCase>();
builder.Services.AddSingleton<BenchmarkPlanService>();
builder.Services.AddSingleton<GenerateBenchmarkPlanUseCase>();

builder.Services.AddSingleton<BenchmarkProjectGenerator>();
builder.Services.AddSingleton<BenchmarkFixtureGenerator>();
builder.Services.AddSingleton<BenchmarkClassGenerator>();
builder.Services.AddSingleton<DotnetCommandRunner>();
builder.Services.AddSingleton<BenchmarkDotNetRunner>(sp =>
{
    var runner = sp.GetRequiredService<DotnetCommandRunner>();
    var artifactsPath = Path.GetFullPath(
        Path.Combine(workspacePath, options.ArtifactsBasePath));
    return new BenchmarkDotNetRunner(runner, artifactsPath);
});

builder.Services.AddSingleton<GenerateBenchmarkForCandidateUseCase>();

builder.Services.AddSingleton<MarkdownReportGenerator>();
builder.Services.AddSingleton<BenchmarkHistoryStore>(sp =>
{
    var artifactsPath = Path.GetFullPath(
        Path.Combine(workspacePath, options.ArtifactsBasePath));
    return new BenchmarkHistoryStore(artifactsPath);
});
builder.Services.AddSingleton<BenchmarkResultParser>();
builder.Services.AddSingleton<AnalyzeBenchmarkResultsUseCase>();
builder.Services.AddSingleton<GeneratePerformanceReportUseCase>();
builder.Services.AddSingleton<PerformanceCopilot.Storage.Local.BaselineStore>(sp =>
{
    var artifactsPath = Path.GetFullPath(
        Path.Combine(workspacePath, options.ArtifactsBasePath));
    return new PerformanceCopilot.Storage.Local.BaselineStore(artifactsPath);
});
builder.Services.AddSingleton<RegressionDetector>();
builder.Services.AddSingleton<CompareWithBaselineUseCase>();

builder.Services.AddSingleton<SessionCandidateStore>();
builder.Services.AddSingleton<SessionPlanStore>();

builder.Services
    .AddMcpServer()
    .WithStdioServerTransport()
    .WithToolsFromAssembly(typeof(Program).Assembly);

await builder.Build().RunAsync();
