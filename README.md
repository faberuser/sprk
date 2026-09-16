# sprk

A Rust implementation of a server emulator for a real-time RPG mobile game client.

## Disclaimer

This project is for educational and preservation purposes only.

- All trademarks, copyrights, and other intellectual property related to the original game and its associated franchise belong to their respective owners.
- This repository does not include any copyrighted game assets, binaries, or master data.
- Use this software at your own risk. The authors assume no responsibility for any damages or legal consequences resulting from its use.

## Implemented / Testing

The following server flows are implemented. Client playthrough testing is ongoing; remaining work within these systems is listed separately below.

### Accounts, quests, and rewards

- **Account basics:** guest authentication, login/logout, in-memory sessions, and account state restored at login.
- **Stamina:** balance tracking, regeneration, purchases, and consumption.
- **Tutorial:** rewards, hero recruitment, scripted battle progress, and reconnect support.
- **Attendance and login rewards:** configurable daily/conditional calendars, accumulated-login milestones, UTC resets, and persistent claims.
- **Achievements and quests:** extracted achievement/subquest definitions, progress from supported gameplay, reward claims, and login/gameplay notifications.
- **Completion rewards:** chapter-star rewards, entitlement-checked clear/newcomer missions, and claims for persisted world-map events.

### Heroes, equipment, and customization

- **Hero collection:** Hero's Inn recruitment, ruby purchases, hero selectors, multiple-hero selectors, and growth tickets.
- **Hero management:** skill learning/upgrades/extensions, awakening trials and purification, transcendence and standard skill pages, limit breaks, bookmarks, avatars, and account-wide equipment/skill presets.
- **Costumes:** normal body-costume purchases and selectors, ownership, equip/unequip, appearance presets, unique-weapon visibility, and gold/EXP bonuses.
- **Equipment:** equip/unequip and swapping, material-based upgrades/awakening, tier changes, option upgrades/rerolls, skill rerolls, enchanting/confirmation, dismantling, and artifact restoration.
- **Soul weapons:** liberation, grade upgrades, ether injection/reinforcement, option renewal/confirmation, transitions, dismantling, and configurable soul-stone restoration with mileage rewards.
- **Runes and loadouts:** hero rune pages, equip/removal with paid preservation, equipment loadout slots, punishment-rune crafting/storage/expansion, and equipment-rune dismantling.
- **Additional progression and customization:** NPC gifts/reward claims, class-buff spending/reset with configurable local point earnings, team-buff upgrades, hair/weapon unlocks from costumes, accessory selectors/positioning, and customization resets.
- **Valance equipment:** crafting, identification, awakening, enchanting, and persistent confirmation choices.

### Inventory, crafting, and shops

- **Inventory:** supported consumables, boxes/selectors, locks, selling/dismantling, equipment storage/expansion, and timed gold/EXP boosters.
- **Specialized items:** EXP flask filling from participating capped heroes and cancellation, pet selectors with persistent collection ownership, accessory selectors, nickname/guild-name tickets, awakening/soul transition tickets, soul growth tickets, transcendence-point potions, and extra-option equipment selectors.
- **Crafting:** recipes, slots, timed completion/cancellation, and collection.
- **Item shops:** table-defined listings and prices, supported currencies, NPC discounts, rotating stock/restocking, and persistent purchase limits.

### Dungeons, raids, and multiplayer

- **Campaign:** persistent battle entries, ownership/party/prerequisite checks, replay rejection, participating-hero EXP and flask filling, table rewards/boosters, selected rewards, and scenario completion.
- **Dispatch and sweeps:** timed campaign dispatch, reserved heroes, cancellation refunds and atomic collection; tower/maze and Shakmeh sweeps with ticket/key costs and mode rewards.
- **Dungeon progression:** tower floors/rewards/resets, party restrictions and reported HP/MP carry-over, day-of-week availability, prison attempts/recharges, treasure-house selection, God King trial opening, and hideout/conquest resets.
- **Battle mode state:** solo raid records/resets, Shakmeh clear rewards/gauge/passives, Eclipse decks, and Ordeal Arena opponent snapshots/nodes/buffs/resurrection with configurable local win points.
- **Bosses and rooms:** persistent world/event-boss HP and scores, rankings, world-boss daily and world/challenge season reward mail, and HTTP co-op room creation/search/join/leave/host transfer. Boss schedules, damage bounds, and challenge damage totals use local rules.

### Arena, guilds, and community

- **Arena:** normal-arena registration/cancellation, offline opponent snapshots, persistent results, local rankings and seasons, and daily/season reward mail.
- **Guilds:** native creation/search/applications, membership and roles, master transfer, settings/notices, disbanding, and withdrawal restrictions.
- **Guild progression:** contributions, attendance/reward mail, guild levels, buildings/investment, skill upgrades/reset/votes, Guild Points, and building-gated guild-shop purchases.
- **Guild battles:** solo guild-raid boss progression, participant kill mail, shared loot purchases and clear-time rankings; guild-arena registration, defense snapshots, offline attacks, records, rankings and season reward mail, using local rules and configurable schedules.
- **Mail:** personal/global inboxes, pagination, expiry, and atomic attachment claims.
- **Friends:** search, invitations, acceptance/removal, daily points, and live notifications.
- **Chat:** native TCP world/channel messages, whispers, guild messages, and reconnect history.

### Configuration and development tools

- **Optional data support:** handlers for configured soul-weapon limit breaks, recipe consumables, and maze aggregate rewards; tower NPC snapshots when enabled. These require supplied definitions and matching client data/settings where applicable.
- **Development tools:** GM/cheat commands for currencies, heroes, levels, unlocks, and equipment.

Equipment-extension configuration and data limitations are documented in [hero-equipment-extensions.md](docs/hero-equipment-extensions.md).
Battle configuration, supported flows, and remaining limitations are documented in [battle-systems.md](docs/battle-systems.md).
Arena/guild configuration and client limitations are documented in [arena-guild.md](docs/arena-guild.md).

## Implementing

This backlog lists remaining implementation, missing data, limitations, and validation work; it is not a priority order. Completed capabilities are listed under **Implemented / Testing**. Route families in parentheses identify the extracted client's corresponding service contracts. An empty success response from the generic fallback does not count as an implemented feature.

### Accounts, quests, and rewards (WIP)

- **Account services:** account linking/recovery, native nickname/country-change services, player inspection/search, remaining native account queries, and token expiry/refresh validation (`user/*`).
- **Missing progression data:** restore actual main-quest definitions (the extracted table contains only a placeholder), original attendance schedules, and world-map event generation/completion rules (`quest/*`, `world_map/*`).
- **Progression integration:** add achievement/mission tracking for future battle modes, collection archives, NPC gifts, and equipment/soul-weapon progression. Connect paid mission entitlements to verified purchases (`clear_mission/*`, `newbie_mission/*`).
- **Seasonal progression and rewards:** King's Pass, monthly hero completion, phase-step rewards, scheduled push rewards, and coupon redemption (`kings_pass/*`, `monthly_hero/*`, `phase_step/*`, `push_reward/*`, `promotion/*`).

### Hero, equipment, and item extensions (WIP)

- **Missing original rules:** supply soul-weapon limit-break definitions and recipe-consumable recipes; these features remain disabled without them. Recover original class-buff point earnings and soul-stone restoration balance.
- **Client-dependent progression:** enhanced transcendence perk levels (this extracted client's page parser stores only selected skill codes), dyes, and legendary-costume progression need corresponding client contracts/data.
- **Unavailable shop stock:** all extracted accessory shop rows have `IsOpen=false`; accessory purchases require valid open stock. Separate paid hair/weapon listings are absent from this extraction.
- **Battle and event dependencies:** event flasks, archive/achievement potions, mode-specific recovery items, and boosters tied to unfinished battle modes/reward calculations remain unsupported.
- **Valance tier upgrades:** recover usable request fields and upgrade rules; the extracted request has no target/material fields or matching definitions.
- **Integration testing:** client playthroughs, appearance-preset interactions, class/team-buff effects in unfinished battle modes, and original random distributions still need verification.

### Dungeons, raids, and multiplayer (WIP)

- **Real-time battles:** implement the native battle service and room socket notifications/readiness, shared combat, reconnects, and verified multiplayer reward allocation. Co-op/Eclipse combat remains unavailable (`party_dungeon/*`, `raid/*`, `eclipse/*`).
- **Missing mode definitions:** supply maze aggregate rewards (empty by default), punishment-raid stage definitions and trigger/bonus rewards, and Karma dungeon rules. Tower NPC battles require matching enabled client/server settings; all extracted towers disable NPCs.
- **Remaining mode behavior:** Eclipse runs/sweeps and original Ordeal matchmaking, scoring, and event rules.
- **Boss scheduling and rewards:** recover original boss rotations/phases and challenge-raid scoring/tie rules; implement event-boss daily kill/season rewards and world-boss achievement rewards/server buffs. Multiplayer challenge rankings depend on the battle service.
- **Further integration and validation:** event-dungeon reset rules, raid dispatch, full battle-mode achievement tracking, and client playthroughs. Combat simulation and verification of client-reported wins/damage remain unimplemented.

### Arena and guilds (WIP)

- **Live PvP and special arenas:** battle-service transport, live matchmaking, friendly duels, ban/pick, World Arena, Lucky Arena, first-tier rewards, and cross-server federation (`match/*`, `global_arena/*`).
- **Guild battle dependencies:** multiplayer raid combat, raid bonus/dummy encounters, chapter-final rewards, full guild-war session rewards/server buffs/flags, and original matchmaking/scoring (`guild_raid/*`, `guild_arena/*`).
- **Conquest/suppression:** restore the missing `RaidIndex=90001` definition and implement its party battle service, combat, rewards, and populated ranking boards. The session reports `NotHeld`; registration fails without spending (`guild_suppress/*`, `guild_ranking_board/*`).
- **Guild integration:** live guild-change notifications, skill effects in battle snapshots/reward calculations, original Guild Point daily caps/bonuses, fuller achievement tracking, and native client playthroughs. Season captions may be blank beyond the client's historical calendar.

### Shops, events, and pets

- **Advanced shops and billing:** remaining event currencies, selection shops, dynamic offers/discounts, paid dungeon access, paid bundles, and platform purchase verification. The extracted paid-product identifiers do not include the original live server's full prices/reward catalog (`shop/*`, `cashshop/*`, `payment/*`).
- **Equipment summons:** free/paid equipment gacha, pickup groups, mileage/pity, and ceiling rewards (`equip_gacha/*`).
- **Events:** calendars, event-step progress/rewards, equipment forging/exchange, event crafting, and roulette rewards (`event/*`, `event_step/*`, `event_equip/*`, `item/event_craft_item`, `item/reward_event_roulette`).
- **Pets:** collection/gacha, eggs/incubators, feeding/interactions, awakening/tier upgrades, house layouts, and exploration missions (`pet/*`).

### Client integration and supporting services

- **Replays and community features:** replay save/list/playback, recommended decks, and records-of-honor rankings (`replay/*`, `recommend_deck/*`, `records_of_honor/*`).
- **Remaining integration:** equipment links in chat, authenticated socket identity, native stamina purchase/recharge routes, and complete login/lobby state for each newly implemented system.
- **Battle services and validation:** multiplayer battle-server callbacks, reconnect/recovery, authoritative result processing, and end-to-end client tests for the implemented flows (`internal/b2g_*`, `internal/b2m_*`).

## Source layout

API handlers and tests are grouped by feature under `src/api`. See the [API layout guide](src/api/README.md) for the folder responsibilities.

## Building the Server

### Prerequisites

- Rust 1.9+ ([Install from https://rustup.rs](https://rust-lang.org/tools/install/))
- SQLite ([bundled with the project](https://www.sqlite.org/download.html))

### Build Steps

```bash
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

Use the unified patcher to patch both `Assembly-CSharp.dll` and `resources.assets` in one command — just point it at your game client root folder:

```batch
scripts\patch.bat "D:\path\to\game\client"
scripts\patch.bat --restore "D:\path\to\game\client"
```

Or via PowerShell:

```powershell
.\scripts\patch.ps1 -ClientPath "D:\path\to\game\client"
.\scripts\patch.ps1 -ClientPath "D:\path\to\game\client" -Restore
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
