# Use a custom illustration model on your client

Replace `active.sprkmodel` in the game's writable model folder to use your own
model with the illustration viewer's **Off / On** button. This changes the
model for your Windows user. It requires no server changes, client DLL patch,
launcher update, or signed release. The launcher leaves this folder alone.

Normal game installations using the same Windows user and Unity storage folder
share this selection. It is independent of the launcher's Local/Public server
profile.

## 1. Choose compatible model files

You need a matching NCNN **`.param` and `.bin` pair**, such as the model files
inside your Upscayl installation. On this machine, Remacri's files are:

```text
D:\ai\Upscayl\resources\models\remacri-4x.param
D:\ai\Upscayl\resources\models\remacri-4x.bin
```

Remacri is already the game's default. Use another compatible pair to try a
different model, or the Remacri pair to explicitly select your installed copy.

The current interface requires:

| Property | Requirement |
| --- | --- |
| Format | NCNN text `.param` and binary weights `.bin` |
| Architecture | RRDB/ESRGAN compatible with the bundled Real-ESRGAN NCNN runtime |
| Input / output tensor names | `data` / `output` |
| Image convention | Three-channel RGB, normalized to [0, 1] |
| Native scale | x4 |
| File limits | Param up to 512 KiB; weights up to 64 MiB |

Some Upscayl models use different architectures or tensor names. Being usable
in Upscayl does not guarantee compatibility here. `.pth`, `.safetensors`, and
ONNX files need conversion; renaming their extensions does not convert them.
The packaging tool checks basic format/size constraints, but actual inference
is the compatibility check.

## 2. Package the pair

Open PowerShell and run these commands from the `sprk-server` repository.
Adjust the source paths for your chosen model. Keep this PowerShell session
open for the following steps.

```powershell
Set-Location -LiteralPath 'D:\kr\sprk\sprk-server'

$paramFile = 'D:\ai\Upscayl\resources\models\remacri-4x.param'
$weightsFile = 'D:\ai\Upscayl\resources\models\remacri-4x.bin'
$packedModel = Join-Path $env:TEMP ('sprk-model-' + [guid]::NewGuid().ToString('N') + '.sprkmodel')

python scripts/DllPatcher/prepare_illustration_runtime.py `
    --pack-model $packedModel `
    --param $paramFile `
    --weights $weightsFile
if ($LASTEXITCODE -ne 0) { throw 'Packaging failed; leave the active model unchanged.' }
```

This command packages the local files without downloading anything or changing
the client. It prints the packaged model's SHA-256. It refuses to overwrite an
existing output file, so the example uses a unique temporary filename.

## 3. Back up and activate it

Close the game first so the viewer and its inference process have released the
old model. You can leave your local server running.

The normal model folder is:

```text
%USERPROFILE%\AppData\LocalLow\sprk\King's Raid\SprkIllustrationAI
```

In the same PowerShell session:

```powershell
$ErrorActionPreference = 'Stop'
$modelDir = Join-Path $env:USERPROFILE "AppData\LocalLow\sprk\King's Raid\SprkIllustrationAI"
$activeModel = Join-Path $modelDir 'active.sprkmodel'
New-Item -ItemType Directory -Path $modelDir -Force | Out-Null

$backupModel = $null
if (Test-Path -LiteralPath $activeModel) {
    $backupModel = $activeModel + '.backup-' + [guid]::NewGuid().ToString('N')
    Copy-Item -LiteralPath $activeModel -Destination $backupModel
    Write-Host "Previous model saved to $backupModel"
}

Copy-Item -LiteralPath $packedModel -Destination $activeModel -Force
Get-FileHash -LiteralPath $activeModel -Algorithm SHA256
```

The printed hash should match the packaging command's hash. Keep your backup
until you have tried the result. Do not put the model in the installed game's
`Managed` folder or edit `default-model.json`.

## 4. Try it in the viewer

Start the game, open an illustration, and select **On**. The original
artwork stays visible while processing. Hover the button for progress or error
details. **Off** restores the original artwork.

On displays the model's full native x4 output. There is no x2 option or
output downsampling. Off restores the original. Turning On again can reuse the
completed disk cache instead of running the model again. A loading bar below
the button shows download/processing percentages and an activity indicator
during preparation and image loading. It disappears when ready, cancelled,
or hidden by Hide UI.

The runtime processes one image at a time with 64-pixel tiles, a 2 GiB
child-process memory limit, and a 120-second inference timeout. A model that
exceeds those limits or fails to load leaves the original artwork visible.

You do not need to clear the cache. Cache keys include the actual model hash,
so replacing the model causes new results to be generated. A nonempty custom
model is preserved even if it is incompatible; the game will not silently
replace it with the default.

## Restore your previous model

Close the game, then copy your backup back to `active.sprkmodel`. In the same
PowerShell session, if a backup was created above:

```powershell
if ($backupModel -and (Test-Path -LiteralPath $backupModel)) {
    Copy-Item -LiteralPath $backupModel -Destination $activeModel -Force
}
```

If you opened a new session, set `$activeModel` and `$backupModel` to the actual
destination and saved backup paths before running the copy.

## Return to the current default, Remacri

Close the game and move your custom `active.sprkmodel` aside. This preserves it
for later use:

```powershell
$modelDir = Join-Path $env:USERPROFILE "AppData\LocalLow\sprk\King's Raid\SprkIllustrationAI"
$activeModel = Join-Path $modelDir 'active.sprkmodel'
if (Test-Path -LiteralPath $activeModel) {
    Move-Item -LiteralPath $activeModel -Destination ($activeModel + '.disabled-' + [guid]::NewGuid().ToString('N'))
}
```

Restart the game and select On. With no active model, the game downloads and
verifies Remacri from the update endpoint captured by your launcher. Your local
update server must be running when using a Local launcher profile.

There is one migration exception: if `active.sprkmodel` exactly matches the old
stock anime model, the game keeps that file but selects
`default-remacri-4x-v1.sprkmodel` instead, downloading it if needed. Other custom
models in `active.sprkmodel` take priority over that migration file. Leave the
migration file alone when swapping custom models.

See [Illustration AI in the patcher guide](../scripts/DllPatcher/README.md#illustration-ai)
for runtime behavior and verification commands.
