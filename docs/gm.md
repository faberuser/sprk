# Private GM administration

GM commands run on a separate listener in the game container, using the same
database and game state. The public game port always returns HTTP 404 for
`/cheat` and `/cheat/*`. The update service does not serve GM commands.

## Portainer setup

Set these stack environment variables, then rebuild/redeploy the game service:

```dotenv
SPRK_GM_KEY=<random secret of at least 32 characters>
SPRK_GM_BIND_IP=127.0.0.1
SPRK_GM_PORT=8082
```

Generate a secret yourself with `python -c "import secrets; print(secrets.token_urlsafe(48))"`.
Use the same secret for the CLI. Do not commit it or distribute it with the client.
An empty key disables the listener; an invalid short key prevents startup.

The default host binding is `127.0.0.1`, accessible only on the Docker host.
Set it to the Docker host's LAN or VPN IP for administration from another machine.

For a server running without Compose, `SPRK_GM_BIND` defaults to
`127.0.0.1:8082`. Compose sets it to `0.0.0.0:8082` inside the container and uses
`SPRK_GM_BIND_IP` to restrict the published host address.

## CLI usage

On Windows, `start_server.bat` loads the repository's `.env`, including
`SPRK_GM_KEY`. Restart it after editing the file. Nonempty terminal environment
variables take precedence. Local startup keeps GM bound to `127.0.0.1:8082`;
the `SPRK_GM_BIND_IP` and `SPRK_GM_PORT` settings above apply to Compose.
The local loader supports single-line literal values with optional quotes and
comments; it does not expand variable references or execute expressions.

```powershell
python gm.py server http://127.0.0.1:8082
python gm.py players
python gm.py players Raider
python gm.py account 42
python gm.py currency --gold 1000
python gm.py --account-id 43 currency --gem 100
```

Commands prompt for the key with hidden input. Alternatively set `SPRK_GM_KEY`
in your terminal environment. The key is never saved in `gm_config.json` or
sent in URL parameters. Redirects are refused to avoid forwarding the key.
Existing configs pointing at port 8080 must be changed with the `server` command.

Player lookup searches username, nickname, or exact numeric AccountId. It returns
up to 100 players; use `players --after <NextAfter>` for the next page.
Accounts appear after their first game login creates the player record.
AccountId stays the same across logins and can target offline players. A player
who is already online may need to log in again to refresh the displayed state.

The interactive menu includes player lookup and account selection. All CLI
player commands target AccountId. Old saved session settings are ignored; select
an account with `python gm.py account <AccountId>`.
Currency commands add only explicitly supplied amounts; they do not set balances.

Use the private LAN/VPN path for administration. For access over an untrusted
network, use a VPN or SSH port forwarding rather than sending the key over HTTP.

## Verification

An empty POST to `https://127.0.0.1/cheat/currency` should return **404**
after deployment (the previous build returned `401 Session expired`).
An admin request on port 8082 without the key should return **401**.
`python gm.py players` with the correct key should list player IDs and names.

Tests: `cargo test private_admin_requires_key` and
`python -m unittest discover -s tests -p test_gm.py`.
