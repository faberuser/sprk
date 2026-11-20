# Client automation

`client/scripts/automation/client_automation.py` controls the real Unity client through the
opt-in `SprkAutomation` assembly built from `client/managed/SprkAutomation`. Python uses its standard library plus
`psycopg` for the test database (`pip install "psycopg[binary]"`). Normal
launches without `SPRK_AUTOMATION_DIR` do not start the bridge.

## Install and launch

The helper is part of every client build. From the repository root:

```powershell
python client/scripts/automation/client_automation.py launch --seed-from sprk
```

`--client <folder>` tests another game folder, for example a copy of the game
with freshly built DLLs, without touching the installed one. Set
`SPRK_HOST_URL=http://127.0.0.1:8080/host.json` in the environment so the test
client can only reach the local server.

The launcher creates a new PostgreSQL database `sprk_auto_<run>` and starts the
release server on ports 8080/9001 against it, with logs in a new
`target/client-automation/<run>` directory, then starts the client at 1280x720.
It never points a newly started test server at the main database. `--seed-from`
clones an existing database into the new one (`sprk` is the one
`start_server.bat` uses; stop that server first, because PostgreSQL only clones
a database nobody is connected to); without it the database starts empty. Databases are created on
`SPRK_AUTOMATION_POSTGRES_URL`, by default the local container that
`start_server.bat` runs (`postgres://postgres:sprk-local@127.0.0.1:54320/postgres`).
Drop old `sprk_auto_*` databases by hand. Build the release server first if
needed. Ports must be free. `--external-server` explicitly reuses an
already-running server; its database is not isolated by the launcher.

`--profile host` selects separate PlayerPrefs keys and a stable test device ID.
Different profiles create distinct guest accounts. Profile launches use copied,
separately named executables to avoid Unity's single-instance check, with
junctions to existing assets and runtime directories. They do not duplicate the
asset archive. These views share patch/download files: do not update or patch
the client while either instance runs. Engine settings and preferences in
third-party assemblies are not fully virtualized.

```powershell
python client/scripts/automation/client_automation.py launch --seed-from sprk --profile host
# Record the run directory printed above before starting the second client.
python client/scripts/automation/client_automation.py launch --external-server --profile guest --database <database URL from the host run's session.json>
```

Test clients are muted: the bridge sets `AudioListener.volume = 0` when it
starts, so test windows stay silent while the user works. Saved sound settings
are not changed. Set `SPRK_AUTOMATION_SOUND=1` before `launch` only for a test
that needs audio. Normal launches never start the bridge and are unaffected.

Fresh accounts retain their normal tutorial/unlocks. Use `new-guest.json` to
reach the guest warning/tutorial. An existing progressed account can run the
login-to-lobby and Portal smoke scenarios.

## Observe, act, and verify

```powershell
python client/scripts/automation/client_automation.py --session <run> snapshot
python client/scripts/automation/client_automation.py --session <run> screenshot
python client/scripts/automation/client_automation.py --session <run> click Portal
python client/scripts/automation/client_automation.py --session <run> text <input-path-or-id> <value>
python client/scripts/automation/client_automation.py --session <run> run client/scripts/automation/scenarios/portal-smoke.json
python client/scripts/automation/client_automation.py --session <run> quit
python client/scripts/automation/client_automation.py --session <host-run> stop-server
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
not rendered visibility. Database assertions run in read-only PostgreSQL
transactions. Queries keep `?` placeholders; write a literal `%` as `%%`:

```json
{
    "action": "assert-db",
    "query": "SELECT count(*) FROM battle_room_members WHERE room=?",
    "params": [1],
    "expected": [[2]]
}
```

Each run retains client/server logs, command requests, observations, PNGs, and
scenario reports. Failed scenarios attempt a screenshot and exit nonzero.
`portal-smoke.json` saves a baseline snapshot before clearing existing errors;
passing it does not mean startup was error-free. Full battle/reward scenarios
must assert both client outcome and server state before claiming success.

There is no unattended AI daemon: the runner executes scenarios; during an
active coding session the assistant can inspect failures, patch code, rebuild,
restart, and rerun them. It does not synthesize successful combat outcomes.

### Battle keyboard settings regression

Launch the test copy with an isolated `battle-keyboard-*` profile. In Login,
send `battle-keyboard-probe` through the Python `request` API with `target=1`
and `text="run"` (`text="init"` only creates the native settings fixture).
The probe checks default OFF, all 48 preset keys/modifiers/labels, custom
persistence across presets and managed-cache reload, duplicates and unbinding,
preference restoration, idempotent settings-row injection and its editor callback.
Synchronous native fixtures also cover normal/Soul Weapon action resolution,
screen-X ordering, inactive/disabled/transparent widget rejection, locked-skill
click delegation and single-back capture cancellation without binding changes.
It leaves the keyboard editor open for a screenshot; allow rendering frames
before capturing. It deliberately bypasses account-dependent `settings.Open`.

This fixture does not establish full battle validation: also test left-to-right
hero ordering, real modifier/key-down edges and reservation/cancellation in a
battle, plus hidden main-party/alternate-party controls. Native key capture can
be exercised by selecting Custom, clicking a binding and using `key SHIFT_TAB`.

In a running battle (same `battle-keyboard-*` profile), `battle-keyboard-live`
reports the battle hook's gate (party open/visible, battle started/enabled),
the key badge text on every hero's skills and `fires` / `firesBeforeStart` counters;
`text="enable"` turns the controls on first. `key` accepts digits and letters,
so `key 1` during combat followed by `battle-keyboard-live` shows whether a
real key press reached a skill. Keep profile names short: the account lock
file path exceeds Windows MAX_PATH with long ones and login then fails with
"This account is open in another window."

## Test a UI change with prefab fixtures (no login)

Most UI changes can be tested without an account, a seeded database or
clicking through menus. A probe renders the one window you changed from its
prefab on the Login screen, fills it with fixture data, calls the changed code
directly and checks the result. One run takes seconds. Use this first. Then
also check the change in the real flow when it depends on account state or
real game data.

**Write the probe** in the feature assembly, next to the feature: one
`public static string Run(string mode)` entry point returning `PASS: ...` or
throwing on the first failed check. `SprkTicketSearchProbe` is the reference:

- `Show(name)` instantiates the window's `PrefabPath` under the `UIRoot`,
  raises its panel depths above the Login screen and assigns atlases.
- Fill the window with fixture data from the tables (for example every opened
  hero from `CreatureDataContainer`), then call the feature's hooks the way the
  game would (`AfterPartyGrid(panel)`).
- Assert with a `Check(condition, message)` helper. Put the measured geometry
  into failure messages and into the `PASS` text, so one run shows what the
  layout actually is.
- Add a `...-show:<Window>` mode that leaves the window open in an interesting
  state, for a screenshot.

**Dispatch it** from `SprkAutomation.Update` like `ticket-search-probe`: require
the Login scene and a dedicated profile prefix (`ticket-search-*`), load the
assembly by name and invoke `Run` with `command.text`.

**Run it:**

1. Close the game. If the user's local server is running, its battle worker
   holds the real game's `Managed` DLLs open (its data folder is a junction to
   `../sprk-client`). That blocks `install`, not the test copy.
2. Build into the test copy without starting it:
   `python -c "import sys; sys.path.insert(0,'.'); from client import dev; dev.prepare_test_copy(dev.GAME, code=True)"`.
   `client/dev.py build` and `install` share `artifacts/client-build`; if
   another `dev.py` process is running, wait for it instead of retrying.
3. Launch: `python client/scripts/automation/client_automation.py launch --client ../sprk-client-test --profile ticket-search-x`.
   That starts an isolated server and database and needs ports 8080/9001 free.
   If the user's server is running, add `--external-server`; a Login-screen
   probe sends nothing to it.
4. Wait for `scene == "Login"` in a `snapshot`, then send the probe through the
   Python API:
   `client_automation.request(client_automation.session(None), "ticket-search-probe", text="party-hero-test:RaidPartySetting")`.
   The result is in `materials`.
5. Take a `screenshot` after a `...-show:` mode. For a control that opens
   something (a dropdown, a popup), use the bridge's `click` on its hierarchy
   path from a `snapshot`; that runs the real open path.
6. Clean up: `quit`, then `stop-server`. `quit` alone leaves the isolated
   server holding port 8080.

**Lessons from earlier probes:**

- The fixture account is partial (`EnsureAccount`): there is no `UserManager`,
  for example. When a native builder needs it, build the fixture items the same
  way without the missing part instead of calling the builder.
- A window shown outside `BaseWindow.Open` can stay at its fade-in start
  (alpha 0) and capture as an empty screen; set its panels' `alpha = 1` in the
  show mode. Opening a popup from inside the probe is unreliable (selecting an
  item closes it); click it through the bridge instead.
- NGUI anchors can rewrite positions and clip regions later; raid hero lists
  re-anchor every frame. After changing a rect, call `ResetAndUpdateAnchors()`
  in the probe and check the change survives. For anchored rects, change the
  anchor offset (`topAnchor.absolute`), not the size.
- To compare spacing with a user's screenshot, measure pixel rows of edges in
  both PNGs (Pillow: brightness steps down one column). This UI is about
  1.7 NGUI units per pixel at 1280x720.
- Run every related probe after a change, including older ones for the same
  feature (`hero-info-test` alongside `party-hero-test:*`), to catch regressions
  in shared code.

## Validation and restoration

Verified on the actual client: launch, screenshot, UI enumeration, native event
click from login to lobby, Portal open/close smoke test, and concurrent client
instances. Startup observations include missing Constant/EventTrigger/DeviceModel
table messages and a certificate-decoding error. Those are captured findings,
not fixed by installing automation. Dragon and story parties use the native
battle worker; configure its executable before starting the server. The old
`setup_native_battle.py` helper is no longer included. Test both clients through combat and settlement, since a
successful invitation/join alone does not verify battle support.

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

The old standalone `test_client_automation.py` suite is no longer included.
Use the real-client scenarios above for automation checks; current build and
packaging tests are listed in [the scripts inventory](../docs/scripts.md).

To disable testing, launch normally without the automation environment
variables; the helper stays inactive.

Read-only material diagnosis: `python client/scripts/automation/client_automation.py renderers`
adds active renderer hierarchy, material name, shader name, support and visibility
to the reply. Rebuild and install the client after changing the bridge. This command
does not change materials or gameplay. Null slots on visible renderers can identify
magenta effects. Asset changes belong in the tracked client build inputs
described in [CI client releases](github-client-updates.md).

The opt-in bridge also accepts `video-probe` through the Python `request` API.
Without parameters it reports native cinematic state, preparation, playback,
decoded frame, dimensions and resolved URL. Set `text` to a valid video-table
code to play that movie through the native manager; `target=1` invokes native
Skip. It does not mark tutorials or stages complete. This probe requires the
automation environment and is unavailable in ordinary client launches.

`reward-boosters-probe` requires Login and a `reward-boosters-*` automation
profile. It recreates the seven episode-unlock markers, six shop markers and a
real gold bonus through the native event/booster managers and reward grid.
Set `target=1` to assert that only the gold bonus appears after repeated
refreshes using Rebel Clause, scenario and campaign battle definitions. The
probe also checks that the hidden event records and normal item/costume
booster visibility are preserved. This tests the shared result UI without
completing battles or changing account rewards.

`honor-ranking-probe` requires Login and an `honor-ranking-*` automation
profile. It loads the native Records of Honor prefab with synthetic ranking
rows. Set `target=1` to verify ranks 1–8 for both Arena leagues, World Boss
and Guild Conquest on both ranking tabs, including row reuse. It also checks
player and guild level/name spacing, centering and shadow labels across
several text lengths. This fixture does not load account portraits or replace
the server ranking regression tests.

`achievement-delta-probe` requires a logged-in account and an
`achievement-delta-*` profile. `target=1` sends `tutorial/begin_tutorial`
through the native requester (with the `Ping` flag, which passes a running
tutorial's service block), so `OnBaseResponse` applies the reply as in play;
poll until the status starts with `DONE` or `FAIL`. The status lists the
`X-SPRK-*` headers sent, the reply size, how many achievements it carried
(`-1`: omitted) and the client's resulting achievement list.
`client/scripts/automation/achievement_delta_check.py <run> <label> [SQL]`
runs SQL on the run's database first (to change the account's progress),
runs the probe and compares the client's list with the server's full list.
A fresh isolated database has no accounts: register one with the login
form's Create an Account (`text` steps take a `text` field).

`eclipse-gauge-probe` requires Login and an `eclipse-gauge-*` profile. Start
with `target=1`, then poll without a target until the returned status is
`PASS` or `FAIL` (allow 90 seconds). It checks the native gauge prefab at
zero, partial and full progress, then verifies animation completion for small
gains, ticket conversion, multiple tickets and the ticket cap. It uses fixture
values without spending account resources; server Eclipse tests separately
check campaign spending, persisted points, conversion and replay protection.

The `ticket-search-probe` command with `text="inventory-bulk-test"` requires
Login and a `ticket-search-*` automation profile. It checks the native ticket
quantity input, Max/Plus/Minus/OK states and ownership at the 100,000-use cap,
material-ticket prefill, and both package description paths at the 1,000-open
cap, including preserved special package limits. It uses fixture counts and
restores modified table values; it does not consume account items. The server
inventory regression tests separately verify redemption and rejected batches.

The same command covers the hero class filter and search rows.
`text="hero-info-test"` checks Hero Info. `text="party-hero-test:<Window>"`
checks a party window's hero list: `BattlePartySetting`, `RaidPartySetting`,
`RaidSinglePartySetting`, `MultiPartySetting`, `AttributeRaidPartySetting`,
`OrdealArenaBattlePartySetting` or `ShakmehPartySetting`. It covers filtering,
clear, rebuild, reopen, row layout and spacing, short labels in narrow lists,
the trim surviving anchor updates and scrolling, and the class list staying on
screen. `party-hero-show:<Window>` runs the same checks and leaves the window
open with a class and search applied, for a screenshot.
