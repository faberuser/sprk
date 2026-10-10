$ErrorActionPreference = 'Stop'
# Register before creating children. Unlike finally, a Windows job also cleans
# them up when the terminal closes or this PowerShell process is terminated.
Add-Type -Path (Join-Path $PSScriptRoot 'LocalServerLifetime.cs')
[LocalServerLifetime]::Enable()
$project = Split-Path $PSScriptRoot -Parent
Set-Location -LiteralPath $project
. (Join-Path $PSScriptRoot 'import_local_env.ps1')
Import-LocalEnv -Path (Join-Path $project '.env')
$updates = Join-Path $project 'client-updates'
if (-not (Test-Path -LiteralPath (Join-Path $updates 'stable\manifest.json'))) {
    throw 'Missing client-updates\stable\manifest.json. Publish a local client update before starting.'
}

foreach ($port in @(8080, 8081)) {
    $probe = [System.Net.Sockets.TcpListener]::new([System.Net.IPAddress]::Any, $port)
    try { $probe.Start() }
    catch { throw "Port $port is already in use. Close the existing local server before running start_server.bat." }
    finally { $probe.Stop() }
}

Write-Host 'Building SPRK...'
& cargo build --release
if ($LASTEXITCODE -ne 0) { throw 'Server build failed; no services were started.' }
$executable = Join-Path $project 'target\release\sprk-server.exe'
$logs = Join-Path $project 'target\local-server'
New-Item -ItemType Directory -Force -Path $logs | Out-Null

$env:SPRK_UPDATES_DIR = $updates
$env:SPRK_SERVICE_MODE = 'updates'
$env:PORT = '8081'
$updateProcess = $null
try {
    $updateProcess = Start-Process -FilePath $executable -WorkingDirectory $project -WindowStyle Hidden -PassThru `
        -RedirectStandardOutput (Join-Path $logs 'updates.log') `
        -RedirectStandardError (Join-Path $logs 'updates-error.log')
    $ready = $false
    for ($attempt = 0; $attempt -lt 40; $attempt++) {
        if ($updateProcess.HasExited) { throw "Update service exited. Check $logs\updates-error.log" }
        try {
            $null = Invoke-RestMethod -Uri 'http://127.0.0.1:8081/updates/stable/manifest.json' -TimeoutSec 2
            $ready = $true
            break
        } catch { Start-Sleep -Milliseconds 250 }
    }
    if (-not $ready) { throw 'Local update service did not become ready on port 8081.' }

    $env:SPRK_SERVICE_MODE = 'game'
    $env:PORT = '8080'
    $env:SERVER_HOST = '127.0.0.1:8080'
    $env:SERVER_HTTPS = 'false'
    $env:SERVER_NAME = 'Local'
    $env:LOGIN_SERVER = 'http://127.0.0.1:8080/'
    $env:GAME_TABLES_PATH = Join-Path $project 'tables'
    $env:CHAT_BIND = '127.0.0.1'
    $env:BATTLE_BIND = '127.0.0.1'
    $env:SPRK_WEBSOCKET_ORIGIN = 'ws://127.0.0.1:8080'
    $env:SPRK_GM_BIND = '127.0.0.1:8082'
    if (-not $env:RUST_LOG) { $env:RUST_LOG = 'info,tower_http=debug' }
    Write-Host 'Game:    http://127.0.0.1:8080'
    Write-Host 'Updates: http://127.0.0.1:8081/updates/stable/manifest.json'
    if ($env:SPRK_GM_KEY) {
        Write-Host 'GM:      http://127.0.0.1:8082 (administrator key required)'
    } else {
        Write-Host 'GM disabled: set SPRK_GM_KEY in .env to enable it.'
    }
    Write-Host 'Choose Local in SprkLauncher. Press Ctrl+C or close this window to stop all local services.'
    & $executable
    if ($LASTEXITCODE -ne 0) { throw "Game server exited with code $LASTEXITCODE." }
} finally {
    if ($null -ne $updateProcess -and -not $updateProcess.HasExited) {
        Stop-Process -Id $updateProcess.Id -ErrorAction SilentlyContinue
        $updateProcess.WaitForExit()
    }
}
