# Client automation

`scripts/client_automation.py` controls the real Unity client through an opt-in
helper installed by DllPatcher. Python uses only its standard library. Normal
launches without `SPRK_AUTOMATION_DIR` do not start the bridge.

## Install and launch

Close test clients before installing. From the server directory:

```powershell
dotnet run --project scripts/DllPatcher -- ../sprk-client --automation-only
python scripts/client_automation.py launch --seed-db sprk.db
```

The launcher starts the release server on ports 8080/9001 with a SQLite backup
in a new `target/client-automation/<run>` directory, then starts the client at
1280x720. It never points a newly started test server at the main database.
Without `--seed-db`, it creates an empty test database. Build the release server
first if needed. Ports must be free. `--external-server` explicitly reuses an
already-running server; its database is not isolated by the launcher.

`--profile host` selects separate PlayerPrefs keys and a stable test device ID.
Different profiles create distinct guest accounts. Profile launches use copied,
separately named executables to avoid Unity's single-instance check, with
junctions to existing assets and runtime directories. They do not duplicate the
asset archive. These views share patch/download files: do not update or patch
the client while either instance runs. Engine settings and preferences in
third-party assemblies are not fully virtualized.

```powershell
python scripts/client_automation.py launch --seed-db sprk.db --profile host
# Record the run directory printed above before starting the second client.
python scripts/client_automation.py launch --external-server --profile guest --database <host-run>/sprk.db
```

Fresh accounts retain their normal tutorial/unlocks. Use `new-guest.json` to
reach the guest warning/tutorial. An existing progressed account can run the
login-to-lobby and Portal smoke scenarios.

## Observe, act, and verify

```powershell
python scripts/client_automation.py --session <run> snapshot
python scripts/client_automation.py --session <run> screenshot
python scripts/client_automation.py --session <run> click Portal
python scripts/client_automation.py --session <run> text <input-path-or-id> <value>
python scripts/client_automation.py --session <run> run scripts/client-scenarios/portal-smoke.json
python scripts/client_automation.py --session <run> quit
python scripts/client_automation.py --session <host-run> stop-server
```

Without `--session`, commands target the latest launch. Use explicit sessions
when two clients run. Snapshots contain scene, active UI paths/IDs/text, enabled
flags, screen coordinates, active relevant behaviour types, and up to 100 recent
errors with stacks. Input values are omitted from snapshots. Local request files
do record text entered by a test. Logs may contain account/session data; keep
run artifacts local.

`click` dispatches the native NGUI click event on the Unity main thread. It does
not verify pointer hit testing, clipping, modal occlusion, or timing-sensitive
press/drag gestures. `native-click <selector>`, `click-at <x> <y>`, and
`key ESC|ENTER|SPACE|TAB` use real Windows input and refuse to send input if they
cannot focus the selected game window. Foreground focus was refused in this
session; UI-event control and in-engine screenshots were verified instead.

Scenarios support `wait`, `wait-ui`, `click`, `native-click`, `text`, `key`,
`snapshot`, `screenshot`, `clear-errors`, `assert-no-errors`, and `assert-db`.
Selectors match an exact label, full hierarchy path, or current instance ID;
ambiguous matches fail. `wait-ui` checks active enabled elements by substring,
not rendered visibility. Database assertions use read-only SQLite connections:

```json
{"action":"assert-db","query":"SELECT count(*) FROM battle_room_members WHERE room=?","params":[1],"expected":[[2]]}
```

Each run retains client/server logs, command requests, observations, PNGs, and
scenario reports. Failed scenarios attempt a screenshot and exit nonzero.
`portal-smoke.json` saves a baseline snapshot before clearing existing errors;
passing it does not mean startup was error-free. Full battle/reward scenarios
must assert both client outcome and server state before claiming success.

There is no unattended AI daemon: the runner executes scenarios; during an
active coding session the assistant can inspect failures, patch code, rebuild,
restart, and rerun them. It does not synthesize successful combat outcomes.

## Validation and restoration

Verified on the actual client: launch, screenshot, UI enumeration, native event
click from login to lobby, Portal open/close smoke test, and concurrent client
instances. Startup observations include missing Constant/EventTrigger/DeviceModel
table messages and a certificate-decoding error. Those are captured findings,
not fixed by installing automation. Multiplayer combat remains unimplemented.

Final installed-patch validation on 2026-09-30:

- Four harness tests passed; DLL reapplication left Assembly-CSharp byte-identical.
- Two simultaneous fresh-profile guest-login scenarios passed. The test database
  recorded distinct accounts 3 and 4 for `test-host-v2` and `test-guest-v2`.
- Login-to-lobby passed for the existing progressed account.
- An initial Portal open/close test passed. The final fresh-login Portal run
  failed on `GetPrevWorldBossSeason failed: prevWorldBossIndex is null.
  currentWorldBossIndex=1`. Its error checkpoint captured a real client error;
  do not treat the earlier pass as evidence that this issue is fixed.
- Failure report and screenshot are retained in
  `target/client-automation/20260930-154523-313b0a/report-cfd87151fdd94e77a755edc39a5cef4c.json`
  and `a7061fce8da14fdaaf083aa6dda1ad34.png` in that directory.

Run harness tests with:

```powershell
python -m unittest discover -s scripts -p test_client_automation.py
```

The patcher keeps timestamped `Assembly-CSharp.dll.before_automation_*` backups
of the currently restored client and helper backups. Reapplying is idempotent.
To disable testing, launch normally without the automation environment variables.
For a complete removal, close all clients and restore the earliest pre-automation
DLL backup from this installation, then remove SprkAutomation.dll. A full normal
DLL repatch may remove the hooks; reapply `--automation-only` afterward.


Read-only material diagnosis: `python scripts/client_automation.py renderers`
adds active renderer hierarchy, material name, shader name, support and visibility
to the reply. Reinstall `--automation-only` after updating the bridge. This command
does not change materials or gameplay. Null slots on visible renderers can identify
magenta effects. Field projectile repair is separate:
`python scripts/repair_field_raid_effect.py <client directory>` (UnityPy required).
Close the client first. It retains original bundle backups; restoring those files
reverts the approximation. A client asset update can overwrite this local repair.
