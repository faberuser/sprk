@echo off
REM Build once, then run local game and launcher update services together.
powershell.exe -NoProfile -ExecutionPolicy Bypass -File "%~dp0scripts\start_local_server.ps1"
pause
