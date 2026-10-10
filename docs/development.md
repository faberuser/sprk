# Development checks

Run these from the repository root. Rust 1.93 matches the Docker build image.

```powershell
cargo clippy --all-targets -- -D warnings
cargo test --all-targets
python -m unittest discover -s tests -v
dotnet run --project launcher/SprkLauncher.Tests
dotnet build launcher/SprkLauncher/SprkLauncher.csproj -warnaserror
dotnet build scripts/DllPatcher/DllPatcher.csproj -warnaserror
dotnet build scripts/DllDisasm/DllDisasm.csproj -warnaserror
```

Rust gameplay tests use isolated SQLite databases. Their decoded table fixture is
loaded once per test process and shared through `Arc`; tests modifying tables
must use `Arc::make_mut` to keep changes local. `SPRK_TABLE_FIXTURE` optionally
selects an alternate fixture directory; by default the repository's `tables/`
directory is used. Missing tables fail setup rather than falling back silently.

Keep tests for protocol compatibility, authorization, currency/inventory
transactions, retry behavior, and persisted state. Avoid testing a separate
implementation of production behavior. For example, booster and costume tests
call the same reward-bonus calculation as battle settlement.
Tests that spend resources seed their own balances rather than relying on the
new-player grant. Two native-worker integration checks remain ignored by default:
they require an external patched Unity worker and a trusted battle fixture.

The Python process-lifetime tests are Windows-only and verify cleanup with
temporary child processes and ephemeral ports. They do not start the game server.
Building the DLL patcher does not patch an installed client; running it against a
client is a separate operation.

Item grant callers use `ItemGrant` with named `index`, `count`, `star`, and
`custom` fields. It is an internal input type, not part of the client protocol.
