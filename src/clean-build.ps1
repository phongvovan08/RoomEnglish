#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Clean and rebuild the RoomEnglish solution to fix Static Web Assets errors.

.DESCRIPTION
    This script thoroughly cleans all build artifacts, caches, and intermediate files
    that can cause "An item with the same key has already been added" errors in
    Static Web Assets. Then optionally rebuilds the solution.

.PARAMETER SkipBuild
    Skip the build step after cleaning.

.PARAMETER Verbose
    Show detailed output during cleaning.

.EXAMPLE
    .\clean-build.ps1
    Clean everything and rebuild.

.EXAMPLE
    .\clean-build.ps1 -SkipBuild
    Only clean without rebuilding.

.EXAMPLE
    .\clean-build.ps1 -ShowDetails
    Show detailed cleaning progress.
#>

[CmdletBinding()]
param(
    [Parameter(Mandatory=$false)]
    [switch]$SkipBuild,
    
    [Parameter(Mandatory=$false)]
    [switch]$ShowDetails
)

$ErrorActionPreference = "SilentlyContinue"

# Colors for output
function Write-ColorOutput($ForegroundColor, $Message) {
    $fc = $host.UI.RawUI.ForegroundColor
    $host.UI.RawUI.ForegroundColor = $ForegroundColor
    Write-Output $Message
    $host.UI.RawUI.ForegroundColor = $fc
}

function Write-Step($Message) {
    Write-ColorOutput "Cyan" "`n==> $Message"
}

function Write-Success($Message) {
    Write-ColorOutput "Green" "✓ $Message"
}

function Write-Info($Message) {
    if ($ShowDetails) {
        Write-ColorOutput "Yellow" "  → $Message"
    }
}

# Ensure we're in the src directory
$srcPath = $PSScriptRoot
if (-not (Test-Path "$srcPath\Web\Web.csproj")) {
    Write-ColorOutput "Red" "Error: Must run from src directory containing Web project"
    exit 1
}

Set-Location $srcPath

Write-ColorOutput "Magenta" @"
╔════════════════════════════════════════════════════════════════╗
║          RoomEnglish Clean Build Script                        ║
║  Fixes: Static Web Assets duplicate key errors                 ║
╚════════════════════════════════════════════════════════════════╝
"@

# Step 1: Clean ClientApp
Write-Step "Cleaning ClientApp caches"
$clientAppPath = "Web\ClientApp"
$foldersToClean = @("dist", "public", ".vite", "node_modules\.vite")

foreach ($folder in $foldersToClean) {
    $fullPath = Join-Path $clientAppPath $folder
    if (Test-Path $fullPath) {
        Write-Info "Removing $fullPath"
        Remove-Item -Recurse -Force -ErrorAction SilentlyContinue $fullPath
    }
}
Write-Success "ClientApp caches cleaned"

# Step 2: Clean Web wwwroot
Write-Step "Cleaning Web wwwroot"
$wwwrootPath = "Web\wwwroot"
if (Test-Path $wwwrootPath) {
    Write-Info "Removing $wwwrootPath"
    Remove-Item -Recurse -Force -ErrorAction SilentlyContinue $wwwrootPath
}
Write-Success "wwwroot cleaned"

# Step 3: Clean all obj and bin directories
Write-Step "Cleaning all obj and bin directories"
Get-ChildItem -Recurse -Directory -Force | Where-Object { 
    $_.Name -eq 'obj' -or $_.Name -eq 'bin' 
} | ForEach-Object {
    Write-Info "Removing $($_.FullName)"
    Remove-Item -Recurse -Force -ErrorAction SilentlyContinue $_.FullName
}
Write-Success "All obj/bin directories cleaned"

# Step 4: Clean StaticWebAssets cache files
Write-Step "Cleaning StaticWebAssets cache"
Get-ChildItem -Recurse -Force -Filter "*.StaticWebAssets.*" | ForEach-Object {
    Write-Info "Removing $($_.FullName)"
    Remove-Item -Force -ErrorAction SilentlyContinue $_.FullName
}
Get-ChildItem -Recurse -Force -Filter "staticwebassets*" | ForEach-Object {
    Write-Info "Removing $($_.FullName)"
    Remove-Item -Force -Recurse -ErrorAction SilentlyContinue $_.FullName
}
Write-Success "StaticWebAssets cache cleaned"

# Step 5: Clean artifacts directory (ROOT CAUSE of duplicate key errors)
Write-Step "Cleaning artifacts directory (main cause of errors)"
$artifactsPath = "..\artifacts"
if (Test-Path $artifactsPath) {
    Write-Info "Removing $artifactsPath"
    Remove-Item -Recurse -Force -ErrorAction SilentlyContinue $artifactsPath
    Write-Success "Artifacts directory removed - this fixes the duplicate key error!"
} else {
    Write-Info "No artifacts directory found (already clean)"
}

# Step 6: Run dotnet clean
Write-Step "Running dotnet clean"
dotnet clean --nologo --verbosity quiet
Write-Success "dotnet clean completed"

Write-ColorOutput "Green" "`n✓ All cleaning completed successfully!`n"

# Step 7: Build if not skipped
if (-not $SkipBuild) {
    Write-Step "Building Web project"
    Write-Info "Running: dotnet build Web/Web.csproj"
    
    dotnet build Web/Web.csproj
    
    if ($LASTEXITCODE -eq 0) {
        Write-Success "Build succeeded!"
    } else {
        Write-ColorOutput "Red" "✗ Build failed with exit code $LASTEXITCODE"
        exit $LASTEXITCODE
    }
} else {
    Write-Info "Skipping build (use without -SkipBuild to build)"
}

Write-ColorOutput "Magenta" "`n╔════════════════════════════════════════════════════════════════╗"
Write-ColorOutput "Magenta" "║                    All Done! ✓                                 ║"
Write-ColorOutput "Magenta" "╚════════════════════════════════════════════════════════════════╝`n"
