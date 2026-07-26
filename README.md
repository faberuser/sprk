# sprk

A Rust implementation of a server emulator for a real-time RPG mobile game client.

## Disclaimer

This project is for educational and preservation purposes only.

- All trademarks, copyrights, and other intellectual property related to the original game and its associated franchise belong to their respective owners.
- This repository does not include any copyrighted game assets, binaries, or master data.
- Use this software at your own risk. The authors assume no responsibility for any damages or legal consequences resulting from its use.

## Progress

- Guest authentication
- Equipment system
- Campaign battles
- Tutorial system (WIP), not implemented:
    - Rewards during tutorial
    - Get hero animation (Kasel, Frey, Cleo, Roi, Clause?)
- Stamina management
- Mail system (WIP)
- Friend system (WIP)

## Features

- User authentication and session management
- Hero collection and management
- Equipment system
- Inventory system
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

# The binary will be at target/release/sprk-server.exe (Windows)
# Or target/release/sprk-server (Linux/Mac)
```

### Running the Server

```bash
# Run the server (default port 8080)
cargo run --release

# Or run the binary directly
./target/release/sprk-server
```

The server will:

1. Create a SQLite database file (`sprk.db`) on first run
2. Initialize all required tables
3. Start listening on `http://0.0.0.0:8080`

### Patching the Client

```
powershell -ExecutionPolicy Bypass -File patch_assets.ps1 -AssetsPath "D:\sprk\sprk Playtest CCBT\sprk_Data\resources.assets" -Mode localhost
```

```
powershell -ExecutionPolicy Bypass -File patch_assets.ps1 -AssetsPath "D:\sprk\sprk Playtest CCBT\sprk_Data\resources.assets" -Mode lan
```
