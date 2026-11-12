# SPRK server

The Rust package, PostgreSQL migrations, server tests and GM CLI live here.
Server tools are grouped under `server/scripts/`: `database/`, `local/`,
`worker/`, `performance/` and `docker/`. Client releases are under
`client/scripts/release/`. Shared tables, configuration, CI and Docker
orchestration files remain at the repository root.

Run commands from the repository root:

```powershell
cargo build --locked
cargo test --locked
python server/gm.py --help
```

The root Cargo workspace keeps build outputs in `target/`. `start_server.bat`
continues to start the local server through `server/scripts/`.

See the root [README](../README.md) for environment setup and the
[database guide](../docs/database-diagnostics.md) for PostgreSQL diagnostics.
