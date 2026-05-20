using PerformanceCopilot.Benchmarking.Models;
using System.Diagnostics;

namespace PerformanceCopilot.Benchmarking.Generation;

public sealed class BenchmarkProjectGenerator
{
    public async Task<CreateBenchmarkProjectResult> CreateAsync(
        CreateBenchmarkProjectRequest request,
        CancellationToken ct = default)
    {
        var projectDir = Path.GetFullPath(request.BenchmarkProjectPath);
        Directory.CreateDirectory(projectDir);

        var csprojPath = Path.Combine(projectDir, $"{request.BenchmarkProjectName}.csproj");
        var programPath = Path.Combine(projectDir, "Program.cs");
        var globalUsingsPath = Path.Combine(projectDir, "GlobalUsings.cs");

        await File.WriteAllTextAsync(csprojPath, BuildCsproj(request), ct);
        await File.WriteAllTextAsync(programPath, BuildProgramCs(), ct);
        await File.WriteAllTextAsync(globalUsingsPath, BuildGlobalUsings(), ct);

        if (request.AddToSolution)
        {
            var solutionDir = Path.GetDirectoryName(Path.GetFullPath(request.SolutionPath))!;
            await RunDotnetAsync(solutionDir, $"sln \"{request.SolutionPath}\" add \"{csprojPath}\"", ct);
        }

        return new CreateBenchmarkProjectResult
        {
            Created = true,
            BenchmarkProjectPath = csprojPath,
            FilesCreated = [csprojPath, programPath, globalUsingsPath],
            PackagesAdded = ["BenchmarkDotNet"],
            ProjectReferencesAdded = request.ReferenceProjects
        };
    }

    private static string BuildCsproj(CreateBenchmarkProjectRequest request)
    {
        var references = string.Join(Environment.NewLine,
            request.ReferenceProjects.Select(r =>
                $"    <ProjectReference Include=\"{r}\" />"));

        var itemGroup = references.Length > 0
            ? $"""
              <ItemGroup>
            {references}
              </ItemGroup>
            """
            : string.Empty;

        return $"""
            <Project Sdk="Microsoft.NET.Sdk">
              <PropertyGroup>
                <OutputType>Exe</OutputType>
                <TargetFramework>{request.TargetFramework}</TargetFramework>
                <Nullable>enable</Nullable>
                <ImplicitUsings>enable</ImplicitUsings>
              </PropertyGroup>
              <ItemGroup>
                <PackageReference Include="BenchmarkDotNet" Version="0.14.*" />
              </ItemGroup>
            {itemGroup}
            </Project>
            """;
    }

    private static string BuildProgramCs() =>
        """
        using BenchmarkDotNet.Running;

        BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args);
        """;

    private static string BuildGlobalUsings() =>
        """
        global using BenchmarkDotNet.Attributes;
        global using BenchmarkDotNet.Jobs;
        global using System;
        global using System.Collections.Generic;
        global using System.Linq;
        """;

    private static async Task RunDotnetAsync(string workingDir, string arguments, CancellationToken ct)
    {
        var psi = new ProcessStartInfo("dotnet", arguments)
        {
            WorkingDirectory = workingDir,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false
        };

        using var process = Process.Start(psi)
            ?? throw new InvalidOperationException("Failed to start dotnet process.");

        await process.WaitForExitAsync(ct);

        if (process.ExitCode != 0)
        {
            var error = await process.StandardError.ReadToEndAsync(ct);
            throw new InvalidOperationException($"dotnet sln add failed: {error}");
        }
    }
}
