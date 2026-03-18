@echo off
REM King's Raid Asset Patcher (Batch/PowerShell)
REM Patches resources.assets to change the QueryHost URL

setlocal enabledelayedexpansion

if "%~1"=="" goto :usage
if "%~1"=="--help" goto :usage
if "%~1"=="/?" goto :usage

set "ASSETS_PATH=%~1"
set "MODE=localhost"

if /i "%~1"=="--restore" (
    set "ASSETS_PATH=%~2"
    goto :restore
)

if /i "%~1"=="--lan" (
    set "MODE=lan"
    set "ASSETS_PATH=%~2"
)

if "%ASSETS_PATH%"=="" goto :usage

echo.
echo ======================================
echo King's Raid Asset Patcher
echo ======================================
echo.

REM Call PowerShell to do the binary patching
powershell -ExecutionPolicy Bypass -File "%~dp0patch_assets.ps1" -AssetsPath "%ASSETS_PATH%" -Mode "%MODE%"

if %ERRORLEVEL% EQU 0 (
    echo.
    echo Patch completed successfully!
    exit /b 0
) else (
    echo.
    echo Patch failed!
    exit /b 1
)

:restore
if "%ASSETS_PATH%"=="" goto :usage
echo.
echo Restoring from backup...
echo.

powershell -ExecutionPolicy Bypass -File "%~dp0patch_assets.ps1" -AssetsPath "%ASSETS_PATH%" -Restore

if %ERRORLEVEL% EQU 0 (
    echo.
    echo Restore completed successfully!
    exit /b 0
) else (
    echo.
    echo Restore failed!
    exit /b 1
)

:usage
echo.
echo King's Raid Asset Patcher
echo.
echo Usage:
echo   patch_assets.bat "path\to\King's Raid_Data\resources.assets"
echo.
echo For LAN/multi-machine (use server IP 26.69.156.42):
echo   patch_assets.bat --lan "path\to\King's Raid_Data\resources.assets"
echo.
echo To restore original:
echo   patch_assets.bat --restore "path\to\King's Raid_Data\resources.assets"
echo.
exit /b 1
