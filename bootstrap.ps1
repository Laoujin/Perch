#Requires -RunAsAdministrator
<#
.SYNOPSIS
    Bootstrap Perch on a fresh Windows machine.
.DESCRIPTION
    Installs .NET 10 SDK if missing, clones Perch and your config repo,
    builds, and runs the first deploy. Idempotent — safe to re-run.
.PARAMETER ConfigRepo
    GitHub slug for your config repo (default: Laoujin/perch-config).
.PARAMETER PerchDir
    Where to clone Perch (default: ~/Perch).
.PARAMETER ConfigDir
    Where to clone your config repo (default: ~/perch-config).
.PARAMETER SkipDeploy
    Clone and build only, don't run deploy.
.EXAMPLE
    # One-liner from a fresh machine:
    irm https://raw.githubusercontent.com/Laoujin/perch/master/bootstrap.ps1 | iex
#>
param(
    [string]$ConfigRepo = 'Laoujin/perch-config',
    [string]$PerchDir = (Join-Path $HOME 'Perch'),
    [string]$ConfigDir = (Join-Path $HOME 'perch-config'),
    [switch]$SkipDeploy
)

$ErrorActionPreference = 'Stop'

function Write-Step($msg) { Write-Host "`n>> $msg" -ForegroundColor Cyan }
function Write-Ok($msg)   { Write-Host "   $msg" -ForegroundColor Green }
function Write-Skip($msg) { Write-Host "   $msg (skipped)" -ForegroundColor DarkGray }

# --- .NET 10 SDK ---
Write-Step 'Checking .NET 10 SDK'
$sdk = dotnet --list-sdks 2>$null | Where-Object { $_ -match '^10\.' }
if ($sdk) {
    Write-Ok "Found: $($sdk[0])"
} else {
    Write-Step 'Installing .NET 10 SDK via winget'
    winget install Microsoft.DotNet.SDK.10 --accept-source-agreements --accept-package-agreements
    if ($LASTEXITCODE -ne 0) {
        Write-Host '   winget failed — falling back to dotnet-install script' -ForegroundColor Yellow
        $installer = Join-Path $env:TEMP 'dotnet-install.ps1'
        Invoke-WebRequest 'https://dot.net/v1/dotnet-install.ps1' -OutFile $installer
        & $installer -Channel 10.0
    }
    # Refresh PATH for this session
    $env:PATH = [Environment]::GetEnvironmentVariable('PATH', 'Machine') + ';' +
                [Environment]::GetEnvironmentVariable('PATH', 'User')
    $sdk = dotnet --list-sdks 2>$null | Where-Object { $_ -match '^10\.' }
    if (-not $sdk) { throw '.NET 10 SDK installation failed' }
    Write-Ok "Installed: $($sdk[0])"
}

# --- Git ---
Write-Step 'Checking git'
if (Get-Command git -ErrorAction SilentlyContinue) {
    Write-Ok "Found: $(git --version)"
} else {
    Write-Step 'Installing git via winget'
    winget install Git.Git --accept-source-agreements --accept-package-agreements
    $env:PATH = [Environment]::GetEnvironmentVariable('PATH', 'Machine') + ';' +
                [Environment]::GetEnvironmentVariable('PATH', 'User')
    if (-not (Get-Command git -ErrorAction SilentlyContinue)) {
        throw 'Git installation failed — install manually and re-run'
    }
    Write-Ok "Installed: $(git --version)"
}

# --- Clone Perch ---
Write-Step "Cloning Perch to $PerchDir"
if (Test-Path (Join-Path $PerchDir '.git')) {
    Write-Skip 'Already cloned'
    git -C $PerchDir pull --ff-only
} else {
    git clone "https://github.com/Laoujin/perch.git" $PerchDir
    Write-Ok 'Cloned'
}

# --- Clone config repo ---
Write-Step "Cloning config repo to $ConfigDir"
if (Test-Path (Join-Path $ConfigDir '.git')) {
    Write-Skip 'Already cloned'
    git -C $ConfigDir pull --ff-only
} else {
    git clone "https://github.com/$ConfigRepo.git" $ConfigDir
    Write-Ok 'Cloned'
}

# --- Build ---
Write-Step 'Building Perch'
dotnet build $PerchDir --nologo -v quiet
if ($LASTEXITCODE -ne 0) { throw 'Build failed' }
Write-Ok 'Build succeeded'

# --- Deploy ---
if ($SkipDeploy) {
    Write-Skip 'Deploy (--SkipDeploy)'
} else {
    Write-Step 'Running first deploy'
    dotnet run --project (Join-Path $PerchDir 'src/Perch.Cli') -- deploy --config-path $ConfigDir
    if ($LASTEXITCODE -ne 0) { throw 'Deploy failed' }
    Write-Ok 'Deploy complete'
}

Write-Host "`n=== Perch is ready ===" -ForegroundColor Green
Write-Host "  Perch:  $PerchDir"
Write-Host "  Config: $ConfigDir"
Write-Host "  Run again: dotnet run --project '$PerchDir/src/Perch.Cli' -- deploy"
