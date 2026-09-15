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
- **Campaign basics:** battle entry/completion, table-based rewards and EXP, dungeon visits, and scenario completion.
- **Stamina:** balance tracking, regeneration, purchases, and consumption.
- **Tutorial:** rewards, hero recruitment, scripted battle progress, and reconnect support.
- **Hero collection:** Hero's Inn recruitment, ruby purchases, hero selectors, multiple-hero selectors, and growth tickets.
- **Hero management:** skill learning/upgrades/extensions, awakening trials and purification, transcendence and standard skill pages, limit breaks, bookmarks, avatars, and account-wide equipment/skill presets.
- **Costumes:** normal body-costume purchases and selectors, ownership, equip/unequip, appearance presets, unique-weapon visibility, and gold/EXP bonuses.
- **Item shops:** table-defined listings and prices, supported currencies, NPC discounts, rotating stock/restocking, and persistent purchase limits.
- **Equipment basics:** equip/unequip with ownership and chest-placement checks.
- **Inventory:** supported consumables, boxes/selectors, locks, selling/dismantling, equipment storage/expansion, and timed gold/EXP boosters.
- **Crafting:** recipes, slots, timed completion/cancellation, and collection.
- **Mail:** personal/global inboxes, pagination, expiry, and atomic attachment claims.
- **Friends:** search, invitations, acceptance/removal, daily points, and live notifications.
- **Chat:** native TCP world/channel messages, whispers, guild messages, and reconnect history.
- **Development tools:** GM/cheat commands for currencies, heroes, levels, unlocks, and equipment.

## WIP / Future Implementation

This backlog includes partial implementations and systems not started yet; it is not a priority order. It was checked against the extracted client's `Assembly-CSharp/NShared` service requests and the server's registered routes/handlers. Route families in parentheses identify the corresponding client contracts. An empty success response from the generic fallback does not count as an implemented feature.

### Accounts, quests, and rewards

- **Account services:** account linking/recovery, nickname/country changes, player inspection/search, remaining native account queries, and token expiry/refresh validation (`user/*`).
- **Attendance and login rewards:** native daily/conditional attendance and login reward flows with table-defined schedules; replace the current sample attendance schedule (`attendance/*`, `logindaily/*`).
- **Achievements and missions:** native achievement checks/rewards, gameplay-driven progress, main/subquests, chapter/world-map completion rewards, clear missions, and newcomer missions; replace the current sample achievement definitions (`achievement/*`, `quest/*`, `clear_mission/*`, `newbie_mission/*`, `world_map/*`).
- **Seasonal progression and rewards:** King's Pass, monthly hero completion, phase-step rewards, scheduled push rewards, and coupon redemption (`kings_pass/*`, `monthly_hero/*`, `phase_step/*`, `push_reward/*`, `promotion/*`).

### Hero, equipment, and item extensions

- **Equipment progression:** upgrading, awakening, tier upgrades, enchanting, option/skill rerolls and confirmation, and artifact restoration (`equip/*`).
- **Soul weapons:** liberation, upgrades, ether injection, reinforcement, limit breaks, option renewal, transitions, and soul-stone restoration/mileage rewards (`equip/soul_*`, `equip/restore_soul_stone`, `equip/get_soul_stone_mileage_reward`).
- **Runes and equipment presets:** rune equip/removal, hero rune pages, per-hero equipment storage slots/swapping, equipment-rune dismantling, and punishment rune crafting/storage (`hero/*rune*`, `equip_storage_slot/*`, `punishment_rune/*`).
- **Further hero progression:** enhanced transcendence perk levels, class buffs, team-level buffs, and NPC gift/friendship rewards outside the Hero's Inn (`hero/learn_hero_transcend_skill_page`, `class_buff/*`, `user/reinforce_team_level_buff`, `npc/*`).
- **Cosmetic customization:** hair/weapon/accessory costume purchases and positioning, customization resets, dyes, and legendary costume progression (`hero/buy_customizing_costumes`, `hero/edit_accessory_costume_position`, `hero/reset_all_customizing_costumes`; costume data).
- **Specialized items:** flask filling/cancellation, recipe consumables, accessory/pet selectors, nickname/guild-name tickets, awakening/soul-weapon transition tickets, extra/unique-option selection, and unsupported potion/booster actions (`item/*`).
- **Valance equipment:** crafting, identification, awakening, enchanting/confirmation, and tier upgrades (`valance/*`).

### Dungeons, raids, and multiplayer

- **Campaign completion:** stricter battle-result validation, participating-hero reward handling, chapter-clear/selected rewards, dungeon resets, sweeps, and dispatch missions (`campaign/*`, `sweep/*`, `dispatch/*`).
- **Dungeon-specific progression:** tower and maze rewards/resets, day-of-week and underground-prison state/keys, treasure-house runs, and God King trials (`campaign/*tower*`, `maze_tower/*`, `dow_dungeon/*`, `under_prison/*`, `treasure_house/*`, `godking_trial/*`).
- **Special battle modes:** Eclipse decks/runs/results, Ordeal Arena nodes/buffs/resurrection, punishment raids, and Shakmeh passive state (`eclipse/*`, `ordeal_arena/*`, `punishment_raid/*`, `shakmeh_dungeon/*`).
- **Co-op rooms and raids:** room creation/search/join/leave, host transfer, readiness/polling, shared battle state, raid resets, and multiplayer reward claims (`party_dungeon/*`, `raid/*`, `campaign/*multiplay*`).
- **World and challenge bosses:** boss HP, scores, rankings, daily/seasonal rewards, and challenge-raid leaderboards (`world_boss/*`, `event_world_boss/*`, `raid/*challenge_raid*`).

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
