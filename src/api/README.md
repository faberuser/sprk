# API layout

Modules are grouped by feature. `mod.rs` declares the groups; `src/main.rs` registers HTTP routes, including the routers provided by `battle`, `community`, `extensions`, `live`, and `services`.

| Folder | Responsibility |
| --- | --- |
| `account` | Authentication, login/logout, lobby state, stamina |
| `battle` | Campaign handlers and battle logic, dungeons, dispatch, rooms, bosses and seasons |
| `community` | Chat, friends, mail, guild management/progression, arena and guild battles |
| `extensions` | Advanced equipment, soul weapons, runes, buffs, cosmetics, NPCs and specialized consumables |
| `heroes` | Hero collection/management, costumes, Hero's Inn and hero presets |
| `inventory` | Item use/storage, equipment actions, crafting and item shops |
| `live` | Non-cash offers, equipment/pet summons, calendars, event crafting/forging/roulette, pet care and exploration |
| `progression` | Attendance, achievements, quests, missions and reward notifications |
| `services` | Replay storage, recommended decks, honor rankings, authenticated battle callbacks and recovery |
| `system` | Shared request parsing, middleware, host/session queries, ping, CDN, fallback and GM tools |
| `tutorial` | Tutorial flow, rewards and reconnect state |

Tests live with their features (`tests.rs`, plus `community/social_tests.rs`). Cross-feature calls use `crate::api::<feature>` imports; implementation files within an existing feature can use `super`.

`battle/campaign_handlers.rs` contains campaign HTTP handlers and response types. `battle/campaign.rs` contains the internal campaign rules. They serve different roles and remain separate.

The folder layout does not change client HTTP paths, database schemas or game behavior.
