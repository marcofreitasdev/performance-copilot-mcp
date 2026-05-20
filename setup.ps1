[CmdletBinding()]
param (
    [switch]$ConfigureClaudeDesktop,
    [switch]$Development,
    [switch]$SkipBuild,
    [string]$WorkspacePath = '${workspaceFolder}'
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$Root = $PSScriptRoot
$SolutionPath = Join-Path $Root 'PerformanceCopilot.slnx'
$McpServerName = 'PerformanceCopilot.McpServer'
$McpServerProject = [System.IO.Path]::Combine($Root, 'src', $McpServerName, "$McpServerName.csproj")
$McpServerExe = [System.IO.Path]::Combine($Root, 'src', $McpServerName, 'bin', 'Release', 'net10.0', "$McpServerName.exe")
$VsCodeMcpJson = [System.IO.Path]::Combine($env:APPDATA, 'Code', 'User', 'mcp.json')
$ClaudeDesktopConfig = [System.IO.Path]::Combine($env:APPDATA, 'Claude', 'claude_desktop_config.json')

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

function Set-McpServer([PSCustomObject]$Config, [string]$RootKey, [PSCustomObject]$Entry) {
    if (-not (Get-Member -InputObject $Config -Name $RootKey -MemberType NoteProperty)) {
        $Config | Add-Member -NotePropertyName $RootKey -NotePropertyValue ([PSCustomObject]@{})
    }

    $Servers = $Config.$RootKey
    if (Get-Member -InputObject $Servers -Name 'performance-copilot' -MemberType NoteProperty) {
        $Servers.'performance-copilot' = $Entry
    } else {
        $Servers | Add-Member -NotePropertyName 'performance-copilot' -NotePropertyValue $Entry -Force
    }

    return $Config
}

function New-ServerEntry([bool]$UseDevelopmentMode, [string]$WorkspaceArgument) {
    if ($UseDevelopmentMode -or -not (Test-Path $McpServerExe)) {
        return [PSCustomObject]@{
            type = 'stdio'
            command = 'dotnet'
            args = @('run', '--project', $McpServerProject, '--', $WorkspaceArgument)
        }
    }

    return [PSCustomObject]@{
        type = 'stdio'
        command = $McpServerExe
        args = @($WorkspaceArgument)
    }
}

Step "Checking prerequisites"
$DotnetVersion = & dotnet --version 2>$null
if (-not $DotnetVersion) {
    Fail ".NET SDK was not found. Install .NET 10 from https://dot.net."
}

$MajorVersion = [int]($DotnetVersion -split '\.')[0]
if ($MajorVersion -lt 10) {
    Warn ".NET $DotnetVersion detected. Performance Copilot requires .NET 10 or newer."
} else {
    Ok ".NET $DotnetVersion"
}

if (-not (Test-Path $SolutionPath)) {
    Fail "Solution file not found: $SolutionPath"
}

if (-not (Test-Path $McpServerProject)) {
    Fail "MCP server project not found: $McpServerProject"
}

if (-not $SkipBuild) {
    Step "Building Performance Copilot"
    Invoke-Dotnet @('restore', $SolutionPath)
    Invoke-Dotnet @('build', $SolutionPath, '-c', 'Release', '--no-restore')
    Ok "Release build completed"
}

$ServerEntry = New-ServerEntry $Development.IsPresent $WorkspacePath

Step "Configuring VS Code MCP"
$VsCodeConfig = Read-JsonObject $VsCodeMcpJson
$VsCodeConfig = Set-McpServer $VsCodeConfig 'servers' $ServerEntry
Write-JsonObject $VsCodeMcpJson $VsCodeConfig
Ok "VS Code MCP configuration updated: $VsCodeMcpJson"

if ($ConfigureClaudeDesktop) {
    Step "Configuring Claude Desktop"
    $ClaudeEntry = [PSCustomObject]@{
        command = $ServerEntry.command
        args = $ServerEntry.args
    }

    $ClaudeConfig = Read-JsonObject $ClaudeDesktopConfig
    $ClaudeConfig = Set-McpServer $ClaudeConfig 'mcpServers' $ClaudeEntry
    Write-JsonObject $ClaudeDesktopConfig $ClaudeConfig
    Ok "Claude Desktop configuration updated: $ClaudeDesktopConfig"
    Warn "Restart Claude Desktop before using the server."
}

Write-Host ""
Write-Host "Performance Copilot MCP is ready." -ForegroundColor Green
Write-Host "Server name: performance-copilot" -ForegroundColor Cyan
Write-Host "VS Code config: $VsCodeMcpJson" -ForegroundColor DarkGray
if ($ConfigureClaudeDesktop) {
    Write-Host "Claude Desktop config: $ClaudeDesktopConfig" -ForegroundColor DarkGray
}
