[CmdletBinding()]
param (
    [switch]$SkipBuild,
    [switch]$ProjectScope,
    [string]$WorkspacePath = ''
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$Root = $PSScriptRoot
$SolutionPath = Join-Path $Root 'PerformanceCopilot.slnx'
$McpServerName = 'PerformanceCopilot.McpServer'
$McpServerProject = [System.IO.Path]::Combine($Root, 'src', $McpServerName, "$McpServerName.csproj")
$McpServerExe = [System.IO.Path]::Combine($Root, 'src', $McpServerName, 'bin', 'Release', 'net10.0', "$McpServerName.exe")
$ProjectClaudeJson = Join-Path $Root '.claude.json'
$GlobalClaudeJson = Join-Path $env:USERPROFILE '.claude.json'

function Step([string]$Message) { Write-Host "`n==> $Message" -ForegroundColor Cyan }
function Ok([string]$Message) { Write-Host "    [OK]   $Message" -ForegroundColor Green }
function Warn([string]$Message) { Write-Host "    [WARN] $Message" -ForegroundColor Yellow }
function Fail([string]$Message) { Write-Host "`n    [FAIL] $Message`n" -ForegroundColor Red; exit 1 }

function Invoke-Dotnet([string[]]$Arguments) {
    & dotnet @Arguments
    if ($LASTEXITCODE -ne 0) {
        Fail "dotnet $($Arguments -join ' ') failed with exit code $LASTEXITCODE."
    }
}

function Read-JsonObject([string]$Path) {
    if (Test-Path $Path) {
        return Get-Content $Path -Raw | ConvertFrom-Json
    }

    return [PSCustomObject]@{}
}

function Write-JsonObject([string]$Path, [PSCustomObject]$Value) {
    $Directory = Split-Path $Path -Parent
    if (-not (Test-Path $Directory)) {
        New-Item $Directory -ItemType Directory -Force | Out-Null
    }

    $Json = $Value | ConvertTo-Json -Depth 20
    [System.IO.File]::WriteAllText($Path, $Json, [System.Text.Encoding]::UTF8)
}

function Set-McpServer([PSCustomObject]$Config, [PSCustomObject]$Entry) {
    if (-not (Get-Member -InputObject $Config -Name 'mcpServers' -MemberType NoteProperty)) {
        $Config | Add-Member -NotePropertyName 'mcpServers' -NotePropertyValue ([PSCustomObject]@{})
    }

    if (Get-Member -InputObject $Config.mcpServers -Name 'performance-copilot' -MemberType NoteProperty) {
        $Config.mcpServers.'performance-copilot' = $Entry
    } else {
        $Config.mcpServers | Add-Member -NotePropertyName 'performance-copilot' -NotePropertyValue $Entry -Force
    }

    return $Config
}

Step "Checking prerequisites"
$DotnetVersion = & dotnet --version 2>$null
if (-not $DotnetVersion) {
    Fail ".NET SDK was not found. Install .NET 10 from https://dot.net."
}
Ok ".NET $DotnetVersion"

if (-not (Test-Path $SolutionPath)) {
    Fail "Solution file not found: $SolutionPath"
}

if (-not $SkipBuild) {
    Step "Building Performance Copilot"
    Invoke-Dotnet @('restore', $SolutionPath)
    Invoke-Dotnet @('build', $SolutionPath, '-c', 'Release', '--no-restore')
    Ok "Release build completed"
}

$Command = if (Test-Path $McpServerExe) { $McpServerExe } else { 'dotnet' }
$Arguments = if (Test-Path $McpServerExe) { @() } else { @('run', '--project', $McpServerProject, '--') }
if ($WorkspacePath) {
    $Arguments += $WorkspacePath
}

$Entry = [PSCustomObject]@{
    command = $Command
    args = $Arguments
}

$ClaudeCli = Get-Command 'claude' -ErrorAction SilentlyContinue
if ($ClaudeCli -and -not $ProjectScope) {
    Step "Registering MCP server with Claude Code"
    & claude @('mcp', 'remove', 'performance-copilot', '--global') 2>$null | Out-Null
    $CliArguments = @('mcp', 'add', 'performance-copilot', '--global', '--transport', 'stdio', $Command) + $Arguments
    $Output = & claude @CliArguments 2>&1
    if ($LASTEXITCODE -eq 0) {
        Ok "Claude Code MCP server registered globally"
        Write-Host ""
        Write-Host "Performance Copilot MCP is ready for Claude Code." -ForegroundColor Green
        exit 0
    }

    Warn "Claude CLI registration failed. Falling back to JSON configuration."
    Warn "$Output"
}

$TargetJson = if ($ProjectScope) { $ProjectClaudeJson } else { $GlobalClaudeJson }
Step "Writing Claude Code configuration"
$Config = Read-JsonObject $TargetJson
$Config = Set-McpServer $Config $Entry
Write-JsonObject $TargetJson $Config
Ok "Claude Code configuration updated: $TargetJson"

Write-Host ""
Write-Host "Performance Copilot MCP is ready for Claude Code." -ForegroundColor Green
