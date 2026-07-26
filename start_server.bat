@echo off
REM Start sprk server with LAN configuration
REM This allows clients on other machines to connect

echo Starting sprk server...
echo Server will be accessible at: 192.168.1.96:8080
echo.

REM Set environment variables
set SERVER_HOST=192.168.1.96:8080
set SERVER_NAME=Private Server
set RUST_LOG=info,tower_http=debug

REM Change to the project directory
cd /d "%~dp0"

REM Start the server
cargo run --release

pause
