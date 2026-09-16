# sprk

A Rust implementation of a server emulator for a real-time RPG mobile game client.

## Disclaimer

This project is for educational and preservation purposes only.

- All trademarks, copyrights, and other intellectual property related to the original game and its associated franchise belong to their respective owners.
- This repository does not include any copyrighted game assets, binaries, or master data.
- Use this software at your own risk. The authors assume no responsibility for any damages or legal consequences resulting from its use.

## Implemented / Testing

The following server flows are implemented. Client playthrough testing is ongoing; remaining work within these systems is listed separately below.

- **Account basics:** guest authentication, login/logout, in-memory sessions, and account state restored at login.
- **Campaign:** persistent battle entries, ownership/party/prerequisite checks, replay rejection, participating-hero EXP and flask filling, table rewards/boosters, selected rewards, and scenario completion.
- **Dispatch and sweeps:** timed campaign dispatch, reserved heroes, cancellation refunds and atomic collection; tower/maze and Shakmeh sweeps with ticket/key costs and mode rewards.
- **Dungeon progression:** tower floors/rewards/resets, party restrictions and reported HP/MP carry-over, day-of-week availability, prison attempts/recharges, treasure-house selection, God King trial opening, and hideout/conquest resets.
- **Battle mode state:** solo raid records/resets, Shakmeh clear rewards/gauge/passives, Eclipse decks, and Ordeal Arena opponents/nodes/buffs/resurrection using local rules.
- **Bosses and rooms:** persistent world/event-boss HP and scores, rankings, world-boss daily and world/challenge season reward mail, and HTTP co-op room creation/search/join/leave/host transfer. Co-op combat remains below.
- **Stamina:** balance tracking, regeneration, purchases, and consumption.
- **Tutorial:** rewards, hero recruitment, scripted battle progress, and reconnect support.
- **Hero collection:** Hero's Inn recruitment, ruby purchases, hero selectors, multiple-hero selectors, and growth tickets.
- **Hero management:** skill learning/upgrades/extensions, awakening trials and purification, transcendence and standard skill pages, limit breaks, bookmarks, avatars, and account-wide equipment/skill presets.
- **Costumes:** normal body-costume purchases and selectors, ownership, equip/unequip, appearance presets, unique-weapon visibility, and gold/EXP bonuses.
- **Item shops:** table-defined listings and prices, supported currencies, NPC discounts, rotating stock/restocking, and persistent purchase limits.
- **Equipment:** equip/unequip and swapping, material-based upgrades/awakening, tier changes, option upgrades/rerolls, skill rerolls, enchanting/confirmation, dismantling, and artifact restoration.
- **Soul weapons:** liberation, grade upgrades, ether injection/reinforcement, option renewal/confirmation, transitions, dismantling, and configurable soul-stone restoration with mileage rewards.
- **Runes and loadouts:** hero rune pages, equip/removal with paid preservation, equipment loadout slots, punishment-rune crafting/storage/expansion, and equipment-rune dismantling.
- **Additional progression and customization:** NPC gifts/reward claims, class-buff spending/reset with configurable local point earnings, team-buff upgrades, hair/weapon unlocks from costumes, accessory selectors/positioning, and customization resets.
- **Specialized items:** EXP flask filling/cancellation, pet/accessory selectors, nickname/guild-name tickets, awakening/soul transition tickets, soul growth tickets, transcendence-point potions, and extra-option equipment selectors.
- **Valance equipment:** crafting, identification, awakening, enchanting, and persistent confirmation choices.
- **Inventory:** supported consumables, boxes/selectors, locks, selling/dismantling, equipment storage/expansion, and timed gold/EXP boosters.
- **Crafting:** recipes, slots, timed completion/cancellation, and collection.
- **Mail:** personal/global inboxes, pagination, expiry, and atomic attachment claims.
- **Friends:** search, invitations, acceptance/removal, daily points, and live notifications.
- **Chat:** native TCP world/channel messages, whispers, guild messages, and reconnect history.
- **Attendance and login rewards:** configurable daily/conditional calendars, accumulated-login milestones, UTC resets, and persistent claims.
- **Achievements and quests:** extracted achievement/subquest definitions, progress from supported gameplay, reward claims, and login/gameplay notifications.
- **Completion rewards:** chapter-star rewards, entitlement-checked clear/newcomer missions, and claims for persisted world-map events.
- **Development tools:** GM/cheat commands for currencies, heroes, levels, unlocks, and equipment.

Equipment-extension configuration and data limitations are documented in [hero-equipment-extensions.md](docs/hero-equipment-extensions.md).
Battle configuration, supported flows, and remaining limitations are documented in [battle-systems.md](docs/battle-systems.md).

## Implementing

This backlog includes partial implementations and systems not started yet; it is not a priority order. It was checked against the extracted client's `Assembly-CSharp/NShared` service requests and the server's registered routes/handlers. Route families in parentheses identify the corresponding client contracts. An empty success response from the generic fallback does not count as an implemented feature.

### Accounts, quests, and rewards (WIP)

- **Account services:** account linking/recovery, nickname/country changes, player inspection/search, remaining native account queries, and token expiry/refresh validation (`user/*`).
- **Progression data and dependencies:** restore actual main-quest definitions (the extracted table contains only a placeholder), original attendance schedules, and world-map event generation/completion rules. Add achievement/mission tracking for future battle modes, collection archives, NPC gifts, and equipment/soul-weapon progression. Connect paid mission entitlements to verified purchases (`quest/*`, `world_map/*`, `clear_mission/*`, `newbie_mission/*`).
- **Seasonal progression and rewards:** King's Pass, monthly hero completion, phase-step rewards, scheduled push rewards, and coupon redemption (`kings_pass/*`, `monthly_hero/*`, `phase_step/*`, `push_reward/*`, `promotion/*`).

### Hero, equipment, and item extensions (WIP)

- **Missing original rules:** soul-weapon limit-break definitions and recipe-consumable recipes. Handlers accept configured definitions, but these features remain disabled without them. Class-buff point earnings and soul-stone restoration use explicitly local rules; original server balance is not reproduced.
- **Client-dependent progression:** enhanced transcendence perk levels (this extracted client's page parser stores only selected skill codes), dyes, and legendary-costume progression need corresponding client contracts/data. Normal body costumes, hair/weapon customization, and accessories are implemented separately.
- **Unavailable shop stock:** all extracted accessory shop rows have `IsOpen=false`; the purchase handler enforces those flags. Accessory selectors still work. Hair/weapon pieces in this extraction are unlocked through costumes rather than separate paid listings.
- **Battle and event dependencies:** event flasks, archive/achievement potions, mode-specific recovery items, and boosters whose battle modes/reward calculations are unfinished remain unsupported. Campaign EXP flasks use participating capped heroes. Pet selectors persist collection ownership; the broader pet system remains below.
- **Valance tier upgrades:** the extracted request has no target/material fields and no matching upgrade rules; it returns a failure without spending items. Other Valance flows are implemented.
- **Integration testing:** client playthroughs, appearance-preset interactions, class/team-buff effects in unfinished battle modes, and original random distributions still need verification.

### Dungeons, raids, and multiplayer (WIP)

- **Real-time battles:** implement the native battle service and room socket notifications/readiness, shared combat, reconnects, and verified multiplayer reward allocation. Co-op/Eclipse combat entry currently fails without charging; room metadata and Eclipse decks are available (`party_dungeon/*`, `raid/*`, `eclipse/*`).
- **Special-mode rules:** maze aggregate reward definitions (configurable but empty by default), missing punishment-raid stage definitions and trigger/bonus rewards, Karma dungeon rules, and Eclipse runs/sweeps. Punishment groups with missing raid definitions reject opening without charging. Ordeal uses local opponent snapshots, buff events, and configurable win points; original matchmaking/scoring/events remain unrecovered. Tower NPC snapshots are supported when enabled, but all extracted towers disable NPCs.
- **Boss scheduling and rewards:** original boss rotations/phases, event-boss daily kill/season rewards, world-boss achievement rewards/server buffs, and native challenge-raid scoring/tie rules. Current schedules, damage bounds, and challenge damage totals are local rules; multiplayer challenge rankings depend on the battle service.
- **Further integration:** event-dungeon reset rules, dispatch for raid modes, full battle-mode achievement tracking, and client playthrough testing. Battle entry/result checks do not simulate combat or verify client-reported wins/damage.

### Arena and guilds

- **PvP arena:** matchmaking/cancellation, battle results, offline matches, ranks, seasons, and global leaderboards (`match/*`, `global_arena/*`).
- **Native guild management:** client-compatible creation/search/applications, acceptance/rejection, roles/kicks, master transfer, settings/notices, and disbanding. The current basic guild CRUD routes need integration with these client contracts (`guild/*`).
- **Guild progression:** contributions, attendance/rewards, guild levels/skills, buildings/investment, and guild currencies/shops (`guild/*`, `shop/*`).
- **Guild battle modes:** raid sessions/loot/rankings, guild arena registration/decks/matches, conquest/suppression sessions/scores, and ranking boards (`guild_raid/*`, `guild_arena/*`, `guild_suppress/*`, `guild_ranking_board/*`).

### Shops, events, and pets

- **Advanced shops and billing:** event/guild currencies, selection shops, dynamic offers/discounts, paid dungeon access, paid bundles, and platform purchase verification. The extracted paid-product identifiers do not include the original live server's full prices/reward catalog (`shop/*`, `cashshop/*`, `payment/*`).
- **Equipment summons:** free/paid equipment gacha, pickup groups, mileage/pity, and ceiling rewards (`equip_gacha/*`).
- **Events:** calendars, event-step progress/rewards, equipment forging/exchange, event crafting, and roulette rewards (`event/*`, `event_step/*`, `event_equip/*`, `item/event_craft_item`, `item/reward_event_roulette`).
- **Pets:** collection/gacha, eggs/incubators, feeding/interactions, awakening/tier upgrades, house layouts, and exploration missions (`pet/*`).

### Client integration and supporting services

- **Replays and community features:** replay save/list/playback, recommended decks, and records-of-honor rankings (`replay/*`, `recommend_deck/*`, `records_of_honor/*`).
- **Remaining integration:** equipment links in chat, authenticated socket identity, native stamina purchase/recharge routes, and complete login/lobby state for each newly implemented system.
- **Battle services and validation:** multiplayer battle-server callbacks, reconnect/recovery, authoritative result processing, and end-to-end client tests for the implemented flows (`internal/b2g_*`, `internal/b2m_*`).

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
