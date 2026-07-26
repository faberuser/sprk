#!/usr/bin/env pwsh
# sprk Client Patcher — DLL + assets in one command
# ===================================================
# Patches the game client's Assembly-CSharp.dll and resources.assets
# to work with the sprk private server (localhost:8080).
#
# Usage:
#   .\patch.ps1 -ClientPath "D:\games\sprk"
#   .\patch.ps1 -ClientPath "D:\games\sprk" -Restore
#
# The -ClientPath should point to the game installation root folder
# (the one containing "King's Raid_Data").

param(
    [Parameter(Mandatory=$true)]
    [string]$ClientPath,

    [Parameter(Mandatory=$false)]
    [switch]$Restore
)

$ErrorActionPreference = "Stop"

# ---------------------------------------------------------------------------
# Resolve paths
# ---------------------------------------------------------------------------
$ScriptDir = Split-Path -Parent $PSCommandPath
$DllPatcherDir = "$ScriptDir\DllPatcher"
$ManagedDir = Join-Path $ClientPath "King's Raid_Data\Managed"
$DllPath = Join-Path $ManagedDir "Assembly-CSharp.dll"
$AssetsPath = Join-Path $ClientPath "King's Raid_Data\resources.assets"

# Replacement URL for localhost (must be 87 bytes with padding)
$REPLACEMENT_URL = [System.Text.Encoding]::UTF8.GetBytes('http://127.0.0.1:8080/Masang_Tokyo/Live/1/host_1.0_304dbdffe094.json                   ')
$ORIGINAL_URL = [System.Text.Encoding]::UTF8.GetBytes('https://kr-apne1-patchsrc.masangsoft.com/Masang_Tokyo/Live/1/host_1.0_304dbdffe094.json')

# ---------------------------------------------------------------------------
# Helper functions
# ---------------------------------------------------------------------------
function Find-BytePattern {
    param([byte[]]$Data, [byte[]]$Pattern)
    $patternLength = $Pattern.Length
    $dataLength = $Data.Length
    for ($i = 0; $i -lt ($dataLength - $patternLength + 1); $i++) {
        $match = $true
        for ($j = 0; $j -lt $patternLength; $j++) {
            if ($Data[$i + $j] -ne $Pattern[$j]) { $match = $false; break }
        }
        if ($match) { return $i }
    }
    return -1
}

function Replace-BytePattern {
    param([byte[]]$Data, [byte[]]$Pattern, [byte[]]$Replacement)
    if ($Pattern.Length -ne $Replacement.Length) {
        Write-Error "Pattern and replacement must be the same length!"; return $null
    }
    $count = 0; $index = 0
    while ($index -lt $Data.Length) {
        $found = Find-BytePattern -Data $Data[$index..($Data.Length-1)] -Pattern $Pattern
        if ($found -eq -1) { break }
        $actualIndex = $index + $found
        for ($i = 0; $i -lt $Replacement.Length; $i++) { $Data[$actualIndex + $i] = $Replacement[$i] }
        $count++; $index = $actualIndex + $Pattern.Length
    }
    return @{ Data = $Data; Count = $count }
}

# ---------------------------------------------------------------------------
# Pre-flight checks
# ---------------------------------------------------------------------------
if (-not (Test-Path $ClientPath)) {
    Write-Error "Client path not found: $ClientPath"; exit 1
}
if (-not (Test-Path $ManagedDir)) {
    Write-Error "Managed dir not found at: $ManagedDir`nMake sure ClientPath points to the game root (contains 'King's Raid_Data')."; exit 1
}

Write-Host "================================================" -ForegroundColor Cyan
Write-Host "  sprk Client Patcher" -ForegroundColor Cyan
Write-Host "================================================" -ForegroundColor Cyan
if ($Restore) {
    Write-Host "  Mode: Restore original files" -ForegroundColor White
} else {
    Write-Host "  Client : $ClientPath" -ForegroundColor White
}
Write-Host "================================================" -ForegroundColor Cyan
Write-Host ""

# ---------------------------------------------------------------------------
# Step 1: Patch / Restore DLL
# ---------------------------------------------------------------------------
if ($Restore) {
    Write-Host "[1/2] Restoring Assembly-CSharp.dll from backup..." -ForegroundColor Yellow
    $backupPath = "$DllPath.backup_before_patch"
    if (Test-Path $backupPath) {
        Copy-Item -Path $backupPath -Destination $DllPath -Force
        Write-Host "  Restored DLL from: $backupPath" -ForegroundColor Green
    } else {
        Write-Warning "  No backup found at: $backupPath — skipping DLL restore."
    }
} else {
    if (-not (Test-Path $DllPath)) {
        Write-Error "Assembly-CSharp.dll not found at: $DllPath"; exit 1
    }
    if (-not (Test-Path $DllPatcherDir)) {
        Write-Error "DLL patcher project not found at: $DllPatcherDir"; exit 1
    }
    Write-Host "[1/2] Patching Assembly-CSharp.dll..." -ForegroundColor Yellow
    Push-Location $DllPatcherDir
    try {
        dotnet run -- "$ClientPath"
        if ($LASTEXITCODE -ne 0) { Write-Error "DLL patcher failed with exit code $LASTEXITCODE"; exit 1 }
    } finally { Pop-Location }
    Write-Host "  DLL patching completed." -ForegroundColor Green
}

Write-Host ""

# ---------------------------------------------------------------------------
# Step 2: Patch / Restore assets
# ---------------------------------------------------------------------------
if ($Restore) {
    Write-Host "[2/2] Restoring resources.assets from backup..." -ForegroundColor Yellow
    $backupPath = "$AssetsPath.backup"
    if (Test-Path $backupPath) {
        Copy-Item -Path $backupPath -Destination $AssetsPath -Force
        Write-Host "  Restored assets from: $backupPath" -ForegroundColor Green
    } else {
        Write-Warning "  No backup found at: $backupPath — skipping assets restore."
    }
} else {
    Write-Host "[2/2] Patching resources.assets..." -ForegroundColor Yellow
    if (-not (Test-Path $AssetsPath)) { Write-Error "File not found: $AssetsPath"; exit 1 }

    # Create backup
    $backupPath = "$AssetsPath.backup"
    if (-not (Test-Path $backupPath)) {
        Write-Host "  Creating backup: $backupPath" -ForegroundColor Cyan
        Copy-Item -Path $AssetsPath -Destination $backupPath -Force
    } else {
        Write-Host "  Backup already exists: $backupPath" -ForegroundColor Gray
    }

    # Read file
    $data = [System.IO.File]::ReadAllBytes($AssetsPath)
    $originalSize = $data.Length

    # Check if already patched to localhost
    $localhostPattern = [System.Text.Encoding]::UTF8.GetBytes('http://127.0.0.1:8080')
    if ((Find-BytePattern -Data $data -Pattern $localhostPattern) -ne -1) {
        Write-Host "  Already patched to localhost. Nothing to do." -ForegroundColor Green
    } else {
        # Find and replace original URL
        $idx = Find-BytePattern -Data $data -Pattern $ORIGINAL_URL
        if ($idx -eq -1) { Write-Error "Original URL not found in file!"; exit 1 }
        $result = Replace-BytePattern -Data $data -Pattern $ORIGINAL_URL -Replacement $REPLACEMENT_URL
        if ($result.Count -eq 0) { Write-Error "Failed to replace URL pattern!"; exit 1 }
        $data = $result.Data
        if ($data.Length -ne $originalSize) { Write-Error "File size changed! Aborting."; exit 1 }
        [System.IO.File]::WriteAllBytes($AssetsPath, $data)
        Write-Host "  Patched $($result.Count) occurrence(s) of the original URL." -ForegroundColor Green
    }

    Write-Host ""
    Write-Host "================================================================" -ForegroundColor Green
    Write-Host "  [OK] Patch successful!" -ForegroundColor Green
    Write-Host "================================================================" -ForegroundColor Green
    Write-Host "  Server: http://127.0.0.1:8080" -ForegroundColor White
    Write-Host "================================================================" -ForegroundColor Green
    Write-Host "  Next steps:" -ForegroundColor White
    Write-Host "  1. Start the private server (sprk-server.exe)" -ForegroundColor White
    Write-Host "  2. Launch the game" -ForegroundColor White
    Write-Host "================================================================" -ForegroundColor Green
    Write-Host ""
}

Write-Host ""
Write-Host "================================================" -ForegroundColor Green
Write-Host "  Client patching complete!" -ForegroundColor Green
Write-Host "================================================" -ForegroundColor Green