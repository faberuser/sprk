# sprk Asset Patcher - PowerShell Script
# Patches resources.assets to change the QueryHost URL

param(
    [Parameter(Mandatory=$true)]
    [string]$AssetsPath,
    
    [Parameter(Mandatory=$false)]
    [ValidateSet('localhost', 'lan')]
    [string]$Mode = 'localhost',
    
    [Parameter(Mandatory=$false)]
    [switch]$Restore
)

# Original URL (87 bytes)
$ORIGINAL_URL = [System.Text.Encoding]::UTF8.GetBytes('https://kr-apne1-patchsrc.masangsoft.com/Masang_Tokyo/Live/1/host_1.0_304dbdffe094.json')

# Replacement URLs (must be 87 bytes each with padding spaces)
$REPLACEMENT_URL_LOCALHOST = [System.Text.Encoding]::UTF8.GetBytes('http://127.0.0.1:8080/Masang_Tokyo/Live/1/host_1.0_304dbdffe094.json                   ')
$REPLACEMENT_URL_LAN = [System.Text.Encoding]::UTF8.GetBytes('http://192.168.1.96:8080/Masang_Tokyo/Live/1/host_1.0_304dbdffe094.json                ')

function Find-BytePattern {
    param(
        [byte[]]$Data,
        [byte[]]$Pattern
    )
    
    $patternLength = $Pattern.Length
    $dataLength = $Data.Length
    
    for ($i = 0; $i -lt ($dataLength - $patternLength + 1); $i++) {
        $match = $true
        for ($j = 0; $j -lt $patternLength; $j++) {
            if ($Data[$i + $j] -ne $Pattern[$j]) {
                $match = $false
                break
            }
        }
        if ($match) {
            return $i
        }
    }
    return -1
}

function Replace-BytePattern {
    param(
        [byte[]]$Data,
        [byte[]]$Pattern,
        [byte[]]$Replacement
    )
    
    if ($Pattern.Length -ne $Replacement.Length) {
        Write-Error "Pattern and replacement must be the same length!"
        return $null
    }
    
    $count = 0
    $index = 0
    
    while ($index -lt $Data.Length) {
        $found = Find-BytePattern -Data $Data[$index..($Data.Length-1)] -Pattern $Pattern
        if ($found -eq -1) {
            break
        }
        
        $actualIndex = $index + $found
        for ($i = 0; $i -lt $Replacement.Length; $i++) {
            $Data[$actualIndex + $i] = $Replacement[$i]
        }
        
        $count++
        $index = $actualIndex + $Pattern.Length
    }
    
    return @{
        Data = $Data
        Count = $count
    }
}

function Restore-Backup {
    param([string]$Path)
    
    $backupPath = "$Path.backup"
    
    if (-not (Test-Path $backupPath)) {
        Write-Error "No backup found: $backupPath"
        return $false
    }
    
    Write-Host "Restoring from: $backupPath" -ForegroundColor Cyan
    Copy-Item -Path $backupPath -Destination $Path -Force
    Write-Host "Restored original file" -ForegroundColor Green
    return $true
}

function Patch-AssetsFile {
    param(
        [string]$Path,
        [string]$Mode
    )
    
    # Select replacement URL
    if ($Mode -eq 'lan') {
        $REPLACEMENT_URL = $REPLACEMENT_URL_LAN
        $serverAddress = "192.168.1.96"
        Write-Host "Using LAN IP: $serverAddress" -ForegroundColor Yellow
    } else {
        $REPLACEMENT_URL = $REPLACEMENT_URL_LOCALHOST
        $serverAddress = "127.0.0.1"
        Write-Host "Using localhost: $serverAddress" -ForegroundColor Yellow
    }
    
    if (-not (Test-Path $Path)) {
        Write-Error "File not found: $Path"
        return $false
    }
    
    # Create backup
    $backupPath = "$Path.backup"
    if (-not (Test-Path $backupPath)) {
        Write-Host "Creating backup: $backupPath" -ForegroundColor Cyan
        Copy-Item -Path $Path -Destination $backupPath -Force
    } else {
        Write-Host "Backup already exists: $backupPath" -ForegroundColor Gray
    }
    
    # Read file
    Write-Host "Reading: $Path" -ForegroundColor Cyan
    $data = [System.IO.File]::ReadAllBytes($Path)
    $originalSize = $data.Length
    Write-Host "File size: $($originalSize.ToString('N0')) bytes" -ForegroundColor Gray
    
    # Check if already patched
    $localhostPattern = [System.Text.Encoding]::UTF8.GetBytes('http://127.0.0.1:8080')
    $lanPattern = [System.Text.Encoding]::UTF8.GetBytes('http://192.168.1.96:8080')
    
    $hasLocalhost = (Find-BytePattern -Data $data -Pattern $localhostPattern) -ne -1
    $hasLan = (Find-BytePattern -Data $data -Pattern $lanPattern) -ne -1
    
    if ($hasLocalhost -or $hasLan) {
        Write-Host "File appears to already be patched!" -ForegroundColor Yellow
        
        if ($hasLocalhost) {
            Write-Host "  Current patch: localhost (127.0.0.1)" -ForegroundColor Gray
        }
        if ($hasLan) {
            Write-Host "  Current patch: LAN IP (192.168.1.96)" -ForegroundColor Gray
        }
        
        # Re-patch if switching modes
        $needsRepatch = $false
        if ($Mode -eq 'lan' -and $hasLocalhost) {
            Write-Host "Re-patching from localhost to LAN IP..." -ForegroundColor Yellow
            $result = Replace-BytePattern -Data $data -Pattern $REPLACEMENT_URL_LOCALHOST -Replacement $REPLACEMENT_URL_LAN
            if ($result.Count -gt 0) {
                $data = $result.Data
                Write-Host "[OK] Re-patched $($result.Count) occurrence(s)" -ForegroundColor Green
                $needsRepatch = $true
            }
        }
        if ($Mode -eq 'localhost' -and $hasLan) {
            Write-Host "Re-patching from LAN IP to localhost..." -ForegroundColor Yellow
            $result = Replace-BytePattern -Data $data -Pattern $REPLACEMENT_URL_LAN -Replacement $REPLACEMENT_URL_LOCALHOST
            if ($result.Count -gt 0) {
                $data = $result.Data
                Write-Host "[OK] Re-patched $($result.Count) occurrence(s)" -ForegroundColor Green
                $needsRepatch = $true
            }
        }
        if (-not $needsRepatch) {
            Write-Host "Already patched with correct settings." -ForegroundColor Green
            return $true
        }
    } else {
        # First time patching - find original URL
        $originalIndex = Find-BytePattern -Data $data -Pattern $ORIGINAL_URL
        
        if ($originalIndex -eq -1) {
            Write-Warning "Original URL not found in file!"
            Write-Host "Looking for: $([System.Text.Encoding]::UTF8.GetString($ORIGINAL_URL))" -ForegroundColor Gray
            return $false
        }
        
        # Verify lengths match
        if ($REPLACEMENT_URL.Length -ne $ORIGINAL_URL.Length) {
            Write-Error "URL lengths don't match!"
            Write-Host "  Original: $($ORIGINAL_URL.Length) bytes" -ForegroundColor Gray
            Write-Host "  Replacement: $($REPLACEMENT_URL.Length) bytes" -ForegroundColor Gray
            return $false
        }
        
        # Replace
        $result = Replace-BytePattern -Data $data -Pattern $ORIGINAL_URL -Replacement $REPLACEMENT_URL
        
        if ($result.Count -eq 0) {
            Write-Error "Failed to replace URL pattern!"
            return $false
        }
        
        $data = $result.Data
        Write-Host "Found and replaced $($result.Count) occurrence(s) of the original URL" -ForegroundColor Green
    }
    
    # Verify size unchanged
    if ($data.Length -ne $originalSize) {
        Write-Error "File size changed! Aborting."
        return $false
    }
    
    # Write patched file
    Write-Host "Writing patched file..." -ForegroundColor Cyan
    [System.IO.File]::WriteAllBytes($Path, $data)
    
    # Success message
    Write-Host ""
    Write-Host "================================================================" -ForegroundColor Green
    Write-Host "  [OK] Patch successful!                                       " -ForegroundColor Green
    Write-Host "================================================================" -ForegroundColor Green
    Write-Host "================================================================" -ForegroundColor Green
    Write-Host "  Server: http://${serverAddress}:8080                        " -ForegroundColor White
    Write-Host "================================================================" -ForegroundColor Green
    Write-Host "  Next steps:                                                 " -ForegroundColor White
    Write-Host "  1. Start the private server (sprk-server.exe)         " -ForegroundColor White
    Write-Host "  2. Launch the game                                          " -ForegroundColor White
    Write-Host "                                                               " -ForegroundColor White
    Write-Host "  No hosts file modification needed!                          " -ForegroundColor White
    Write-Host "================================================================" -ForegroundColor Green
    Write-Host ""
    
    return $true
}

# Main execution
try {
    if ($Restore) {
        $success = Restore-Backup -Path $AssetsPath
    } else {
        $success = Patch-AssetsFile -Path $AssetsPath -Mode $Mode
    }
    
    if ($success) {
        exit 0
    } else {
        exit 1
    }
} catch {
    Write-Error "An error occurred: $_"
    exit 1
}
