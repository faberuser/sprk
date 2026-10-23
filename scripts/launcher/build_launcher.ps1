param(
    [string]$ClientPath = (Join-Path $PSScriptRoot '../../../sprk-client'),
    [string]$ManifestUrl = 'http://127.0.0.1:8081/updates/stable/manifest.json',
    [string]$PublicKeyFile = ''
)
$ErrorActionPreference = 'Stop'
$repoPath = Split-Path (Split-Path $PSScriptRoot -Parent) -Parent
$projectPath = Join-Path $repoPath 'launcher/SprkLauncher/SprkLauncher.csproj'
$publishPath = Join-Path $repoPath 'artifacts/launcher'
$resolvedClient = (Resolve-Path -LiteralPath $ClientPath).Path
if (-not (Test-Path -LiteralPath (Join-Path $resolvedClient "King's Raid.exe"))) {
    throw "ClientPath must contain King's Raid.exe"
}
# Get-Content attaches PowerShell provider metadata to its string output. Windows
# PowerShell 5.1 can serialize that as an object instead of the required PEM string.
$publicKey = if ($PublicKeyFile) {
    [System.IO.File]::ReadAllText((Resolve-Path -LiteralPath $PublicKeyFile).Path)
} else { '' }
$iconExtractor = Join-Path $PSScriptRoot 'extract_exe_icon.py'
$iconPath = Join-Path $repoPath 'launcher/Assets/KingsRaid.ico'
if (Test-Path -LiteralPath $iconExtractor) {
    python $iconExtractor (Join-Path $resolvedClient "King's Raid.exe") $iconPath
    if ($LASTEXITCODE -ne 0) { throw 'Game icon extraction failed' }
} elseif (-not (Test-Path -LiteralPath $iconPath)) {
    throw 'The launcher icon and icon extractor are both missing.'
}
dotnet publish $projectPath -c Release -r win-x64 --self-contained true -o $publishPath
if ($LASTEXITCODE -ne 0) { throw 'Launcher publish failed' }
Copy-Item -LiteralPath (Join-Path $publishPath 'SprkLauncher.exe') -Destination (Join-Path $resolvedClient 'SprkLauncher.exe')
$configPath = Join-Path $resolvedClient 'sprk-launcher.json'
$config = [ordered]@{
    ManifestUrl = $ManifestUrl
    GameExecutable = "King's Raid.exe"
    ManifestPublicKeyPem = $publicKey
    AutoLaunch = $true
    SelectedProfile = 'local'
    Profiles = @(
        [ordered]@{ Id = 'local'; Name = 'Local'; HostUrl = 'http://127.0.0.1:8080/host.json'; ManifestUrl = 'http://127.0.0.1:8081/updates/stable/manifest.json' }
    )
}
if ($ManifestUrl -ne $config.Profiles[0].ManifestUrl) {
    # Preserve a supplied manifest without guessing the separately hosted game URL.
    $config.Profiles += [ordered]@{ Id = 'custom'; Name = 'Existing server'; HostUrl = ''; ManifestUrl = $ManifestUrl }
    $config.SelectedProfile = 'custom'
}
# An explicit URL/key configures a new installation. Preserve existing player
# configuration during an ordinary rebuild.
if (-not (Test-Path -LiteralPath $configPath) -or $PSBoundParameters.ContainsKey('ManifestUrl') -or $PublicKeyFile) {
    $encoding = New-Object System.Text.UTF8Encoding($false)
    [System.IO.File]::WriteAllText($configPath, ($config | ConvertTo-Json -Depth 6) + [Environment]::NewLine, $encoding)
}
Write-Host "Launcher: $(Join-Path $resolvedClient 'SprkLauncher.exe')"
Write-Host "Configuration: $configPath"
