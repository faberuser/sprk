@echo off
REM Build once, then run local game and launcher update services together.
powershell.exe -NoProfile -ExecutionPolicy Bypass -File "%~dp0server\scripts\local\start_local_server.ps1"
pause
