# Battle systems

The server imports 43 table groups and 78 HTTP request/response contracts into
`tables/BattleSupport.json`. `scripts/export_battle_data.py` regenerates this file
from the decoded tables, original JIT string pools, and extracted C# source.

## Implemented flows

- Campaign entry persists its party, stage, difficulty, selected rewards, and costs.
  An identical entry retry returns its saved response. Completion requires an active,
  unexpired entry and rejects mismatched stages, invalid survivors, and repeated results.
  Rewards, progression, and completion commit together. Only participating heroes gain EXP.
- Dispatch supports eligible ordinary campaign/boss stages that use stamina. It reserves
  heroes, spends all run costs at entry, marks finished jobs ready for collection, and
  refunds unplayed runs on cancellation. Rewards are calculated from tables when collected;
  this is a timed local simulation in which completed runs win. Raid dispatch is disabled.
- Sweeps require a previous clear at the requested difficulty, consume tickets and keys,
  and grant dungeon plus tower/maze or Shakmeh rewards. Eclipse sweeps are disabled.
- Tower floors cannot be skipped; floor/complete rewards and paid resets persist. Tower
  resets follow extracted daily/weekly/monthly definitions. Party class restrictions and
  floor opening times are checked. Configured tower types retain reported hero HP/MP;
  defeated heroes require a reset, including after a first-floor loss. Enemy HP/MP persists
  on retries of the same floor. Resets are rejected during active battles. Prison attempts, weekday
  availability, treasure-house selection, God King opening, and hideout/conquest resets
  have server-side costs and state checks.
- Solo raids store clear records and support paid resets, hero-level requirements, and
  table-defined party bans. Shakmeh checks predecessor clears and party bans, grants
  extracted first/regular rewards, and persists its capped gauge and passive selection.
  Entering the final boss consumes the full gauge once; completing or losing clears its
  passives. Punishment opening rejects groups whose referenced raid definitions are missing
  before spending a key.
- World/event bosses share persistent HP, record account damage, and expose rankings.
  World-boss daily score rewards and closed world/challenge season rank rewards are
  delivered through mail. A claim ledger and the mail insert share one transaction.
  Mail supports World Boss Point attachments; their balances survive reconnects.
- Co-op rooms support persistent metadata, capacity checks, joining/leaving, host-only
  changes, host transfer, heartbeats, expiry, and reports. A player can occupy one room.
- Eclipse decks validate ownership and duplicate heroes. Ordeal supports tier selection,
  cached local opponents, sequential nodes, battle entry/completion, buff choices,
  resurrection, refreshes, area rewards, and final rating rewards. Event/end nodes follow
  the client's direct-completion flow. Repeated node rewards are rejected.
- Login/lobby restore dungeon masks, towers, keys, dispatch, currencies, and sweep records.
  New SQLite tables/columns are added automatically; existing campaign clears seed the
  richer progress state. GM reset clears battle sessions/rooms while retaining boss
  reward ledgers and scores to prevent repeat claims.

## Configurable local rules

Edit `tables/BattleRules.json` and restart the server. Times use UTC.

| Setting | Meaning |
| --- | --- |
| `BattleExpirySeconds` | How long an entered campaign battle remains valid; default four hours. |
| `DispatchSecondsPerBattle` | Dispatch duration per run; default 60 seconds. |
| `DispatchMaxRepeat`, `DispatchMaxSlots` | Dispatch limits; defaults 100 runs and five slots. |
| `RoomTimeoutSeconds` | Inactive room-member expiry; default 120 seconds. |
| `SeasonEpoch`, `SeasonDays` | Local repeating season boundaries; default seven days. Keep these stable after scores exist. |
| `WorldBossMaxHp` | Shared world-boss starting HP. Event bosses use extracted HP. |
| `WorldBossMaxDamagePerSecond` | Upper bound on reported damage, based on server elapsed time. This does not verify combat. |
| `BossTicketsPerBattle` | Ticket cost when the dungeon table has no boss-ticket cost; default one. |
| `DungeonKeyDailyDefaults` | Daily balances for listed keys. World-boss tickets retain purchased/recovered surplus. Unlisted keys receive no free balance. |
| `PrisonRechargeGem` | Local prison recharge price; default 100 rubies. |
| `UnderPrisonMaxDailyAttempts` | Default prison daily attempt limit; default three. |
| `TowerCarryCreatureTypes` | Tower type IDs that retain reported HP/MP; defaults to Pit (1) and Labyrinth (2). |
| `OrdealArenaWinPoint` | Local points per battle win; default 100. |
| `MazeTowerRewards` | Optional daily aggregate claims: rows with `Index`, `Towers` (objects with `TowerIndex` and `Floor`), and `RewardIndex`. Empty by default because original definitions were unavailable. |

Ordeal opponents are snapshots of other local accounts, falling back to the requesting
account when no opponent exists. Event nodes offer buffs. Refresh fails without payment
when it cannot produce a different snapshot. This is a local substitute for the original
matchmaking and event distribution. The shared normal/Ordeal season endpoint supplies
calendar data; it does not implement PvP matchmaking.

Tower NPC queries use cached local account snapshots when a tower has `UseNpc=true`,
with one opponent per floor retained until the reset period/count changes. All extracted
towers currently set `UseNpc=false`. This preserves the client table setting; enabling
NPC battles also requires matching client data.

Shakmeh uses the extracted 600-point cap and final-boss projectile-group pool. Passive
selection is uniform without replacement, retained across reconnects. Consuming the
gauge on final-boss entry (including losses) is a local rule.

## Remaining work

This is **not a complete implementation of every battle mode**.

- Co-op combat and Eclipse require the native real-time battle protocol and a battle
  service. Their HTTP entry returns a failure without spending resources. HTTP rooms alone
  do not provide ready/deck socket synchronization or shared combat. Multiplayer loot is
  not granted from unverified client submissions.
- Punishment groups reference raid IDs 1001–1008, absent from the extracted `Raid` table.
  Playable stage mappings, trigger/bonus rewards, and Karma dungeon rules need recovery.
  These groups remain disabled; the server does not invent mappings or consume opening keys.
- Bosses use local season windows, not the original rotations, phases, or cross-server
  rankings. Event-boss participation/killing rewards are supported; daily kill and seasonal
  event rewards, world-boss achievement tiers, and server buffs remain unfinished.
  Challenge leaderboards currently sum reported damage; original scoring/tie rules and
  multiplayer team rankings need further work.
- Event-dungeon resets and some mode recovery items remain disabled. Original maze aggregate
  rewards require configuration. In-game visual/state transitions need client playthroughs.
- Entry validation prevents rewards without a matching saved battle, but winning and damage
  still come from client reports. Authoritative combat simulation is a separate requirement.

## Validation

`cargo test` includes battle regressions for mismatched/replayed results, party ownership,
EXP isolation, reconnects, dispatch timing/refunds/reservations, room ownership/host transfer,
tower progression/sweeps/reset recovery/HP reports/NPC caching, Shakmeh gauge/passives,
punishment opening without missing definitions, selected rewards, God King keys, Eclipse deck forgery, Ordeal
events/battle proof, boss HP/tickets, and mail settlement/claims. These checks exercise server
behavior; they do not replace a playthrough with the Unity client.

After `cargo build --release`, run `python scripts/smoke_battle.py` for native HTTP
login/lobby, entry retry, room persistence, restart recovery, result replay rejection,
and dispatch cancellation checks using a temporary database and ports.
