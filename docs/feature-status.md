# Feature Status

## Implemented / Testing

The following server flows are implemented. Client playthrough testing is ongoing; remaining work within these systems is listed separately below.

### Accounts, quests, and rewards

- **Account basics:** guest authentication, login/logout, in-memory sessions, and account state restored at login.
- **Stamina:** authenticated native snapshots, table-priced purchases/key recharges, transactional consumption, persistent recharge counters, and Chicken regeneration.
- **Tutorial:** rewards, hero recruitment, scripted battle progress, and reconnect support. Skipping tutorials records the World Tree story gate (14000), allowing the normal map route from 1-17 to 1-18 without granting battle clears or rewards.
- **Attendance and login rewards:** configurable daily/conditional calendars, accumulated-login milestones, UTC resets, and persistent claims.
- **Achievements and quests:** extracted achievement/subquest definitions, progress from supported gameplay, reward claims, and login/gameplay notifications.
- **Mission UI:** lobby category availability follows the client table's quest requirements. Achievement and Guideline subquest claims accept native repeated form fields as well as JSON arrays, including Claim All batches. Request `Steps` contains each mission's last claimed step (zero before its first claim); the server validates saved progress and awards the next step.
- **Completion rewards:** chapter-star rewards, entitlement-checked clear/newcomer missions, and claims for persisted world-map events. Chapter reward claims restore through the native login key `chapterRewardInfos`, preserving GET marks after reconnect.

### Heroes, equipment, and customization

- **Hero collection:** Hero's Inn recruitment, ruby purchases, hero selectors, multiple-hero selectors, and growth tickets.
- **Hero management:** skill learning/upgrades/extensions, awakening trials and purification, transcendence and standard skill pages, limit breaks, bookmarks, avatars, and account-wide equipment/skill presets.
- **Costumes:** normal body-costume purchases and selectors, ownership, equip/unequip, appearance presets, unique-weapon visibility, and gold/EXP bonuses.
  - Single-costume purchases accept the native client's duplicated `BuyGem`/`BuyGold`/`BuyMileage` quote, validate only table-priced currencies, and debit only those currencies. Forged payable prices still fail.
  - Full and shop-only patching preserves real body/hair/weapon/accessory ownership checks and enables direct sales of non-default outfits (including event/legend outfits) and accessories. Existing prices remain; unpriced outfits/standalone hair or weapon parts cost 3,000 rubies and unpriced accessories cost 500. Default appearances and parts bundled with body costumes retain their existing unlock rules. Client getters and server table loading apply the same pricing policy; no automatic ownership grants.
  - Ungrouped outfits can be purchased in the Dressing Room. Only the shop's grouped hero tiles exclude them, preventing missing-group lookup failures.
- **Equipment:** equip/unequip and swapping, material-based upgrades/awakening, tier changes, option upgrades/rerolls, skill rerolls, enchanting/confirmation, dismantling, and artifact restoration.
- **Soul weapons:** liberation, grade upgrades, ether injection/reinforcement, option renewal/confirmation, transitions, dismantling, and configurable soul-stone restoration with mileage rewards.
- **Runes and loadouts:** hero rune pages, equip/removal with paid preservation, equipment loadout slots, punishment-rune crafting/storage/expansion, and equipment-rune dismantling.
- **Additional progression and customization:** NPC gifts/reward claims, class-buff spending/reset with configurable local point earnings, team-buff upgrades, hair/weapon unlocks from costumes, accessory selectors/positioning, and customization resets. Accessory position requests accept the native extra URL-escape layer and string-encoded coordinates as well as plain JSON numbers, with finite-value and positive-scale validation.
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
- **Chat:** native TCP world/channel messages, whispers, guild messages, reconnect history, verified equipment links, and session-key socket authentication (requires the supplied client patch).

### Shops, events, and pets

- **Non-cash shops:** remaining shop currencies, configurable ruby/gold bundles, persistent selection stock/restocking, purchase limits, timed offers, shared discount limits, and table-priced dungeon access.
- **Summons:** free, currency and validated-ticket draws; category/pickup pools; step-up discounts/rewards; configurable pity, ceiling and final rewards; pet summons and duplicate souls.
- **Events:** configurable calendars, personal/daily/shared step contributions and claims, event crafting, currency/item roulette, equipment forging and exchange.
- **Pets:** shared collection ownership, selectors/rewards, egg supply and incubation, incubator purchases/slot expansion, feeding/interactions/gifts, awakening/soul tier upgrades, house/avatar selection, and timed exploration.
- **Persistence and validation:** atomic costs/rewards, ownership/capacity checks, claim replay protection, login/lobby snapshots, and restart recovery. Missing original rules use explicit local definitions.

### Client integration and supporting services

- **Replays:** persistent save/list/fetch APIs, participant visibility, duplicate detection, and bounded native playback data storage.
- **Community records:** clear-party/equipment snapshots and recommended decks by level/time; score-backed honor rankings, seasons, own ranks, and ranker popups.
- **Battle-service integration:** authenticated registration/heartbeat, campaign claims and result processing, normal-arena callbacks, duplicate-result protection, and persistent recovery. Local/offline gameplay remains configurable.
- **Login/lobby integration:** shared stamina balances/counters and service-run recovery state; HTTP/TCP and process-restart tests.

### Configuration and development tools

- **Optional data support:** handlers for configured soul-weapon limit breaks, recipe consumables, and maze aggregate rewards; tower NPC snapshots when enabled. These require supplied definitions and matching client data/settings where applicable.
- **Development tools:** GM/cheat commands for currencies, heroes, levels, unlocks, and equipment.

Equipment-extension configuration and data limitations are documented in [hero-equipment-extensions.md](hero-equipment-extensions.md).
Battle configuration, supported flows, and remaining limitations are documented in [battle-systems.md](battle-systems.md).
Arena/guild configuration and client limitations are documented in [arena-guild.md](arena-guild.md).
Shop, summon, event, and pet configuration is documented in [shops-events-pets.md](shops-events-pets.md).
Supporting API configuration, the required chat patch, and battle-service contracts are documented in [supporting-services.md](supporting-services.md).

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
- **Cosmetic stock:** local table loading opens extracted accessory stock using the pricing policy above. Hair/weapon parts bundled with body costumes retain their ownership prerequisites.
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

### Shops, events, and pets (limitations)

- **Billing (excluded):** real-money checkout, platform receipt verification, subscription billing, and payment callbacks remain unavailable (`cashshop/*`, `payment/*`).
- **Original live data:** restore historical server prices, reward-pool assignments, schedules, and marketing triggers. Other historical summon banners require configuration; promotional web content/artwork is not hosted.
- **Client integration:** native playthroughs, event/shop visibility settings, and pet combat-bonus integration remain. Missing pet supply/tier/exploration rules and summon pity use configurable local definitions.

### Client integration and supporting services (limitations)

- **Native client validation:** build/apply the supplied chat SessionKey patch and run Unity playthroughs for replay playback, recommendations, honor UI, stamina, and reconnect flows. Popup dungeon metadata and cross-server honor aggregation remain incomplete.
- **Battle runtime:** integrate a compatible combat-server executable/adapter; live transport/host transfer, simulation/integrity checks, Eclipse/shared-raid/guild-suppression callbacks, and full combat buff/stat snapshots remain. Unsupported callbacks explicitly fail. Existing local/offline results are not combat-verified.
- **Stamina fidelity:** reconcile original non-Chicken timed regeneration/reset schedules and temporary/pet stamina-cap buffs with the existing configurable local balances.

- Campaign reward responses expose Raider EXP through the native `ExpResultsByGetHero` field so the active client applies reward EXP immediately; hero EXP remains in `HeroExpResults`.

- Auto equip preserves Unity repeated `HeroPartIndex` / `EquipItemSlotIndex` fields in order, applying every client-selected upgrade in one transaction rather than retaining only the final slot.

- Local campaign policy: successful campaign and campaign-boss clears grant team EXP equal to the stage base hero EXP once per clear (not per hero or hero booster). `ExpResult` refreshes the client immediately; recruitment EXP remains separate. Failed/duplicate completions do not grant it.
