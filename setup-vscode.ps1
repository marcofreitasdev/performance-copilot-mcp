[CmdletBinding()]
param (
    [switch]$Development,
    [switch]$SkipBuild,
    [string]$WorkspacePath = '${workspaceFolder}'
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$Root = $PSScriptRoot
$Setup = Join-Path $Root 'setup.ps1'

if (-not (Test-Path $Setup)) {
    Write-Host "setup.ps1 was not found." -ForegroundColor Red
    exit 1
}

$Arguments = @{
    WorkspacePath = $WorkspacePath
}

if ($Development) {
    $Arguments.Development = $true
}

if ($SkipBuild) {
    $Arguments.SkipBuild = $true
}

& $Setup @Arguments
