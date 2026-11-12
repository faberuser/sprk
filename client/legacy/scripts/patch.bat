@echo off
REM sprk Client Patcher — DLL + assets in one command (Batch wrapper)
REM Patches the game client's Assembly-CSharp.dll and resources.assets
REM to work with the sprk private server.
REM
REM Usage:
REM   patch.bat "D:\games\sprk"
REM   patch.bat --restore "D:\games\sprk"
REM
REM The first argument should be the game installation root folder
REM (the one containing "King's Raid_Data").

setlocal enabledelayedexpansion

if "%~1"=="" goto :usage
if "%~1"=="--help" goto :usage
if "%~1"=="/?" goto :usage

set "MODE_PS="
set "CLIENT_PATH=%~1"

if /i "%~1"=="--restore" (
    set "MODE_PS=-Restore"
    if "%~2"=="" (
        echo Error: --restore requires a client path.
        goto :usage
    )
    set "CLIENT_PATH=%~2"
)

REM Path to the PowerShell script (same dir as this batch)
set "PATCH_PS1=%~dp0patch.ps1"

if not exist "%PATCH_PS1%" (
    echo Error: patch.ps1 not found at %PATCH_PS1%
    exit /b 1
)

echo.
echo ================================================
echo   sprk Client Patcher
echo ================================================
echo   Client path: %CLIENT_PATH%
if defined MODE_PS (
    echo   Mode:        Restore
) else (
    echo   Mode:        Patch ^(localhost^)
)
echo ================================================
echo.

powershell -ExecutionPolicy Bypass -File "%PATCH_PS1%" -ClientPath "%CLIENT_PATH%" %MODE_PS%

if %ERRORLEVEL% EQU 0 (
    echo.
    echo Client patching completed successfully!
    exit /b 0
) else (
    echo.
    echo Client patching failed!
    exit /b 1
)

:usage
echo.
echo sprk Client Patcher
echo.
echo Usage:
echo   patch.bat "path\to\game\client"
echo   patch.bat --restore "path\to\game\client"
echo.
exit /b 1