# sprk

A Rust implementation of a server emulator for a real-time RPG mobile game client.

## Disclaimer

This project is for educational and preservation purposes only.

- All trademarks, copyrights, and other intellectual property related to the original game and its associated franchise belong to their respective owners.
- This repository does not include any copyrighted game assets, binaries, or master data.
- Use this software at your own risk. The authors assume no responsibility for any damages or legal consequences resulting from its use.

## Features

- Accounts, quests, and rewards
- Heroes, equipment, and customization
- Inventory, crafting, and shops
- Events, summons, and pets
- Dungeons, raids, and multiplayer
- Arena, guilds, and community
- Replays, rankings, and supporting services
- Configuration and development tools

## Source layout

- `src/api/`: account, gameplay, GM, and protocol handlers
- `src/models/` and `src/tables/`: client response types and table loading
- `src/updates.rs`: launcher update delivery
- `launcher/`: Windows launcher and its checks
- `scripts/`: client patching, publishing, and local server tools
- `tests/`: Python tool and Windows process-lifetime checks

See [the scripts inventory](scripts/README.md) for active CI and development tools.

## Building the Server

### Prerequisites

- Rust 1.93+ ([Install from https://rustup.rs](https://rust-lang.org/tools/install/))
- SQLite ([bundled with the project](https://www.sqlite.org/download.html))

### VS Code Rust analysis

Install the analyzer and standard-library sources for your active Rust toolchain:

```bash
rustup component add rust-analyzer rust-src
```

The workspace settings use this toolchain's `rust-analyzer` from `PATH` instead
of the extension's bundled version, keeping analysis compatible with the compiler.
The parent SPRK workspace explicitly links `sprk-server/Cargo.toml`. After changing
these settings, run **rust-analyzer: Restart server** in VS Code.

### Build Steps

```bash
# Build in release mode
cargo build --release

# The binary will be at target/release/sprk-server.exe (Windows)
# Or target/release/sprk-server (Linux/Mac)
```

### Running the Server

`SPRK_SERVICE_MODE=game` (the default) runs the game services. Use `updates` to
serve client update manifests/files without initializing the game, or `all` to
run both. See [client updates and Portainer setup](docs/client-updates.md) for the
publishing script, download endpoints, and shared-image container stack.

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

Use the unified patcher to patch both `Assembly-CSharp.dll` and `resources.assets` in one command — just point it at your game client root folder:

```batch
scripts\client\patch.bat "D:\path\to\game\client"
scripts\client\patch.bat --restore "D:\path\to\game\client"
```

Or via PowerShell:

```powershell
.\scripts\client\patch.ps1 -ClientPath "D:\path\to\game\client"
.\scripts\client\patch.ps1 -ClientPath "D:\path\to\game\client" -Restore
```

This will:

1. Run the DLL patcher (`scripts/DllPatcher`) — creates a backup `Assembly-CSharp.dll.backup_before_patch` on first run
2. Patch `resources.assets` to redirect the query host URL to your local server (`http://127.0.0.1:8080`)
3. Both done in one command with just the client folder path

#### DLL Patcher CLI

`DllPatcher` and `DllDisasm` also accept the client root as a CLI argument:

```bash
dotnet run --project scripts/DllPatcher -- "D:\path\to\game\client"
dotnet run --project scripts/DllDisasm -- "D:\path\to\game\client"
```

## Credits

Thanks to Shamomylle/keirakoro/zanezonair for providing clients for this project.
