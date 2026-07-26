@echo off
REM Start sprk server with localhost configuration
REM This is for testing on the same machine as the server

echo Starting sprk server (localhost mode)...
echo.
echo Before connecting, make sure you've patched the game client:
echo   scripts\patch.bat "path\to\game\client"
echo.
echo Server will be accessible at: 127.0.0.1:8080
echo.

REM Set environment variables
set SERVER_HOST=127.0.0.1:8080
set SERVER_NAME=Local
set RUST_LOG=info,tower_http=debug

REM Change to the project directory
cd /d "%~dp0"

REM Start the server
cargo run --release

pause
