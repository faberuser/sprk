@echo off
REM Start King's Raid Private Server with localhost configuration
REM This is for testing on the same machine as the server

echo Starting King's Raid Private Server (localhost mode)...
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
