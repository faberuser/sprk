# King's Raid Private Server

A Rust implementation of a private server for the King's Raid mobile game client.

## Disclaimer

This project is for educational and preservation purposes only. It is intended for use with legally owned copies of the game client. The game's service has ended (End of Service/EoS), and this server allows players to continue enjoying the game privately.

## Progress

- Guest authentication
- Equipment system
- Campaign battles
- Tutorial system (dead end)
- Stamina management

## Features

- User authentication and session management
- Hero collection and management
- Equipment system
- Campaign battles
- Guild system
- Mail system
- Friend system
- Attendance/daily rewards
- Achievement tracking
- Stamina management
- Tutorial system
- GM/Cheat commands for testing

## Building the Server

### Prerequisites

- Rust 1.9+ ([Install from https://rustup.rs](https://rust-lang.org/tools/install/))
- SQLite ([bundled with the project](https://www.sqlite.org/download.html))

### Build Steps

```bash
# Navigate to the server directory
cd server

# Build in release mode
cargo build --release

# The binary will be at target/release/kings-raid-server.exe (Windows)
# Or target/release/kings-raid-server (Linux/Mac)
```

### Running the Server

```bash
# Run the server (default port 8080)
cargo run --release

# Or run the binary directly
./target/release/kings-raid-server
```

The server will:

1. Create a SQLite database file (`kings_raid.db`) on first run
2. Initialize all required tables
3. Start listening on `http://0.0.0.0:8080`

### Patching the Client

```
python scripts/patch_assets.py "D:\\Games\\KING\'s RAID Playtest\\King\'s Raid_Data\\resources.assets"
```
