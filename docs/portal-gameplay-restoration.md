# Portal gameplay restoration audit

The target is the original gameplay, including multiplayer. Offline substitutes
do not satisfy that target. Restoring all 23 menu categories has not restored all
23 categories' gameplay. No complete gameplay restoration is claimed here.

This audit uses the server source, decoded tables, and extracted client in this
workspace. There is no supplied original battle-server source/build, network
capture, or fuller historical data backup. Existing local rules listed below
predate this audit; they must not be mistaken for recovered original rules.

## Category coverage

"Implemented" below means a server implementation exists, not that a native
client playthrough has verified every operation. Detailed limits remain in
[feature status](feature-status.md), [battle systems](battle-systems.md), and
[arena/guild systems](arena-guild.md).

| Portal category | Implementation and outstanding work |
| --- | --- |
| Shop | Non-cash purchases, summons, and currencies implemented; historical stock/pricing needs recovery. Real-money billing remains excluded. |
| Adventure Notice | Native content navigation; historical event schedules/promotional content are incomplete. |
| Chapter | Campaign entry/results, progression, rewards and dispatch implemented; native playthroughs and remaining story/event data need validation. |
| Valance | Crafting, identification, awakening and enchanting implemented; tier-upgrade request fields/rules are missing. |
| Hall of Heroes | Hero progression, prison/day dungeons and related inventory flows implemented; native navigation and dungeon playthroughs need validation. |
| Challenge Tower | Floor progression, resets and rewards implemented; original maze aggregate rewards are missing. |
| Arena | Normal/Ordeal local flows implemented; live matchmaking, ban/pick, friendly, World/Lucky Arena and original season/scoring behavior are incomplete. |
| Raid | Normal Fire solo combat verified. Hard Dragon has 80 reconstructed definitions; Fire solo combat/loss verified and shared stage progression repaired. Field Raid has 16 reconstructed definitions; Infira wave-1 solo combat/loss/exit verified, later waves and other encounters remain. HTTP rooms and native party join/deck/readiness/host-transfer messages exist; private invitations, timeout synchronization, shared combat and multiplayer reward allocation remain. |
| World Boss | Damage records, persistent HP, rankings and some reward mail implemented; original rotations, phases, achievement rewards and buffs are incomplete. |
| God King's Temple | Trial entry and soul-weapon progression implemented; optional limit-break data and original restoration balance are incomplete. |
| Eclipse | Native deck serialization and offline run lifecycle implemented, with transactional tickets/rewards and team progression. Online service, combat buffs and native playthroughs remain. |
| Shakmeh | Entry, gauge, passives and rewards implemented using documented local rules; original-rule fidelity and native playthroughs remain. |
| Challenge Raid | Solo score/ranking support exists; original scoring, multiplayer combat and team rankings are incomplete. |
| Guild | Management, local Guild War and solo guild raid implemented; original live guild battles and Conquest are incomplete. |
| Library | Native navigation exists; a menu entry is not evidence of complete scenario/archive playback or reward coverage. Needs client playthrough. |
| World Map | Campaign navigation/progression implemented; original world-map event generation is missing. |
| Orvel | Town navigation and services implemented in individual feature handlers; requires native service-by-service playthrough. |
| Guild Territory | Headquarters, buildings, training/skills and shop state implemented; battle effects, Conquest boards and notifications are incomplete. |
| Pets | Collection, incubation, progression, exploration implemented with configurable local rules; combat bonuses and original rule recovery remain. |
| Technomagic Raid | Solo raid validation/progression exists; shared multiplayer combat remains missing. |
| Treasure House | Dungeon selection and entry/reward state implemented; native playthrough and original rotation fidelity remain. |
| Apocalypsion Raid | Ten missing Boss/Karma raid definitions are reconstructed in a shared client/server overlay. Native level-1 Boss progression and Karma entry/results/shard settlement are implemented; Boss shard/infinite configurations and native playthroughs remain. |
| Rune Crafting | Crafting, storage and expansion implemented; acquisition through Apocalypsion depends on restoring that mode. |

## Verified blockers and client evidence

- `tables/BattleSupport.json` has 129 Raid rows, none for IDs 1001–1008.
  Its eight PunishmentRaid rows reference those missing IDs. The optional
  `tables/ReconstructedRaids.json` overlay now fills these gaps without changing
  recovered rows; the client patcher embeds the same file.
- `tables/ArenaGuildSupport.json` has GuildSuppressChapter and
  GuildSuppressSession references to raid 90001, also absent from Raid data.
- `src/main.rs` starts HTTP and chat listeners. There is no native battle socket
  listener. `src/api/services/internal.rs` is an HTTP integration adapter, not a
  combat runtime. `src/api/battle/rooms.rs::validate_battle` rejects multiplayer
  entry before charging resources.
- The extracted `NShared/BattleLoginReq.cs`, `BattleStartReq.cs`, room protocols
  and `NGame2/NBattleContext/BattleContext.cs` describe parts of the native
  protocol. A compatible service can be reverse-engineered from this evidence,
  but packet schemas alone do not implement combat, synchronization, reconnect,
  reward allocation, or prove original server-rule fidelity.
- Eclipse is **not exclusively online** in this client.
  `SoulWeaponManagement.BeginEclipse` calls
  `BattleContext.RequestBeginEclipse(..., false)`. That method posts
  `campaign/begin_campaign` without HeroIndices, expects EclipseBattleInfo, and
  initializes EclipseContext/OfflinePlay. Its online branch connects to the
  battle server. Implementing only `eclipse/begin_eclipse` would not repair the
  default Portal entry flow. The server now routes Eclipse campaign requests
  to a separate lifecycle that constructs EclipseBattleInfo from saved decks.

## Repair in this change

The extracted `EclipseDeckSetting.OnClickBattleStart` writes HeroIndices using
`string.Join(",", ...)`. `JM_NShared_EclipseDeckResult` writes DeckIndex as a
string. The server previously required numeric DeckIndex and a JSON array
string for HeroIndices, so native deck saves failed. It also returned the JSON
array string, which the client's comma parser cannot read correctly.

The server now accepts native string indices and comma-separated hero IDs,
returns comma-separated IDs, and normalizes legacy saved array strings on
reads. It retains legacy API input compatibility, ownership checks, hero order,
duplicate checks across decks, and server-generated hero snapshots. Decks are
sorted and required to be contiguous because the native client addresses them
by `DeckIndex - 1`. Invalid replacements leave saved decks intact.

Regression coverage exercises native save/read/info responses, forged stats,
invalid and unowned IDs, duplicate heroes/decks, index gaps, transaction rollback,
and legacy saved-deck reads.

### Native Eclipse run lifecycle

`src/api/battle/eclipse.rs` implements the existing client's offline branch;
online requests are still rejected before ticket consumption. It imports 90
Eclipse wave metadata rows through the battle exporter, prepares owned hero
snapshots, charges 1–5 tickets once per run, and persists run/team state. Native
save-result requests transition between teams; campaign completion returns
EclipseDungeonInfo. End/abandon settles rewards in the same transaction as a
unique run claim. Entry/end retries do not repeat charges or rewards. Run IDs
survive GM state resets. Active runs prevent deck replacement and conflicting
campaign/dispatch entry.

Stage order uses increasing chapter/dungeon IDs from the extracted Eclipse
stages. Start-wave selection and wave reward rows come from EclipseStart and
EclipseReward; the final stage repeats as described by the client updater.
Normal rewards use each team's cleared-wave range, with the trailing reward
block repeated beyond the reward table. Settlement also includes the best
team's reward row when present, multiplied by entry tickets. This settlement
reconstruction is inferred from client reward previews and is not verified
against an original server. It must not be described as exact original balance.

Remaining limits: no Unity playthrough yet; empty guild/account/class/pet buff
snapshots; client-reported results are bounded but not combat-verified; the
existing configurable battle expiry applies; reported progress is capped by
the available EclipseStart records and a conservative elapsed-time check.
Server state persists across reconnect, but the native reconnect UI still
needs validation (it also holds a transient EclipseDungeonInfo object).
An expired run can be abandoned to release the account without another charge.
Online Eclipse, sweeps and multiplayer transport are still unfinished.

Eclipse validation at its completion: all 170 Rust tests passed, including five Eclipse tests. The exported
90 EclipseWave rows have unique keys, and the existing table groups and HTTP
contracts are unchanged. No in-game verification is claimed.

## Remaining implementation sequence

1. Recover the native battle transport framing, session authentication and packet
   lifecycle from extracted code. Add loopback client-contract tests before
   advertising a battle endpoint.
2. Implement room notifications/readiness, party snapshots, native battle start,
   shared combat, result callbacks, disconnect recovery and host transfer.
   Validate with at least two running clients and persistent reward tests.
3. Validate Eclipse's native offline flow in Unity, including reconnect UI,
   combat buffs and reward fidelity, then implement its online branch without
   replacing online play with offline play.
4. Reconstruct remaining mode-specific definitions using client evidence and
   explicitly documented assumptions, as authorized by the user. Validate the
   Punishment Boss/Karma flows in Unity, then restore Boss shard/infinite modes.
   Original server-only rules cannot be verified from absent evidence.
5. Restore remaining arena/guild/boss features and replace existing local rules
   only when supported by recovered evidence. Perform a full native Portal
   playthrough, including rewards, reconnects and multiplayer state.

## Reconstructed Punishment Boss raids

The user authorized reasonable inferred values where original data is absent.
This pass reconstructs IDs 1001-1008 at level 1. It does not substitute local
combat for a multiplayer mode: `PunishmentRaidContext` already implements
`OfflinePlay.IDataProvider`, returns OfflinePlay from GetBattlePlay, and returns
false from IsOnline. `PunishManagment.StartPunishmentBattle` uses
`contents/begin_content`; settlement uses `contents/end_content`. These native
contracts are now exported and routed through the shared transactional battle
lifecycle, with native OpenPunishmentRaidInfos arrays in end responses.

### Evidence and assumptions

- Inferred mappings: 1001-1005 map to chapter 70000, dungeons 1-5;
  1006-1008 map to chapter 70001, dungeons 1-3. These existing BattleType 47
  stages align with the Punishment group chapters, dragon/boss ordering and
  creature identities. Clear-check IDs 1101-1105 and 2001-2003 follow the
  extracted reward conditions. These mappings are reconstructions, not recovered
  Raid rows.
- Inferred Raid defaults: one player, four main heroes, no sub-party,
  minimum hero level 90, level 1, no online-single connection, no individual
  Raid reward or gem reset. Decorative fields unavailable in the extraction
  remain empty. The server and patcher consume the same JSON and preserve any
  recovered original definitions with matching ID/level.
- Recovered opening rules: DungeonType Boss is 2, not 0 (Scenario).
  `PunishmentGroup.OpenStaminaCount` and NPunish.Helper.GetOpenStaminaCount
  specify 6,000 stamina initially, then cumulative table increments per clear.
  The prior one-key opening was incorrect. Group state is independent; reopening
  clears only that group's raid records and preserves its daily clear counter.
- Inferred lifecycle: reset at midnight UTC, successful final-boss completion
  closes that group's run and increments its daily clear counter. Sub-boss
  clears do not increment the counter. A loss permits retry without another
  opening charge. Reset/open is blocked during an active battle.
- Recovered restrictions: at most two dragon clears in group 101001;
  Apocalypsion receives affixes 5001-5004 for uncleared dragons. Solenis receives
  6001/6002 for cleared subordinate bosses. Requests must match those affixes.
- Reconstructed reward policy: select the most specific satisfied
  PunishmentRaidReward clear combination. Active final-boss rows are used once;
  inactive RewardType 2 subordinate rows are enabled as a documented fallback.
  The inactive RewardType 1 main reward is not stacked. Actual item amounts and
  reward rolls use the extracted RewardIndex data. Original payout balance has
  not been verified against a historical server.

Both groups have native request regression coverage, including opening cost,
missing-definition protection, loss/retry, duplicate completion, affix checks,
group isolation, login restoration, reset protection and escalating reopening
cost. Login and native result responses preserve the client's complete cross-group
clear cache, because UserOpenPunishmentRaidManager.Set replaces that cache. The client
patch compiles, and a standalone runtime check verifies all eight parsed rows
and preservation of existing definitions. No Unity playthrough is claimed.
At this Boss milestone, Karma, nonzero flask options, sub-party configurations, higher raid levels and
multiplayer transport remain outside this completed slice. Results still rely
on the existing client-result validation rather than server-simulated combat.

To tune inferred Raid values, edit `tables/ReconstructedRaids.json`, rerun
`dotnet run --project scripts/DllPatcher -- ../sprk-client --portal-only`, and
restart the client and server. The server reads this file at startup; the
patcher embeds it into the client DLL.

Validation for this pass: 172 full-suite Rust tests passed; all four Punishment
tests passed again after the final retry/cache/login fixes. Client patch runtime
parsing passed, and reapplication produced a byte-identical DLL. The installed
client DLL has a timestamped pre-restoration backup in its Managed directory.

## Karma continuation

The overlay now also reconstructs Raid 1101 -> chapter 70101/dungeon 1 and
1102 -> chapter 70201/dungeon 1, using the KarmaRaidIndex and ChapterIndex from
the two PunishmentGroup Karma rows. RaidType is KarmaRaid (19). Four owned
heroes, level-90 minimum and level-1 metadata follow the Boss fallback defaults.
The client already implements the looping stages and card-selection combat;
no replacement battle engine is installed.

`src/api/battle/karma.rs` implements DungeonType Karma (1), independent opening
state alongside Boss state, native contents entry, and transactional settlement.
Opening charges the extracted 3,000 stamina. The exporter includes KarmaDungeon,
KarmaGaugeExponential, KarmaReward, KarmaRewardExponential, ShardFlaskItem and
eight KarmaWave records with creature counts. Wave gauge follows the client's
PunishmentShardGaugeWindow.ExpCalculator, including float32 arithmetic, stage
NextLevel loops, cumulative multipliers and final partial-wave kill credit.

Selected non-default flasks must be owned and unlocked. Entry snapshots the
available count; settlement consumes only filled flasks, grants their mapped
shards, and sends remaining gauge to the group's unlimited default flask.
Default flasks cost no inventory item, matching the client UI's default supply.
Result arrays include consumption and grants so native inventory updates apply.
Progress can settle after a loss, as expected for a survival mode. Duplicate end
requests cannot grant a second payout. Login restores Karma and Boss states.

Explicit inferred rules: one opening permits one settled run, ending closes it,
and reset expires at midnight UTC. Any cleared wave grants one roll each of the
group's extracted KarmaFixRewardIndex and KarmaBonusRewardIndex. The original
server's scaling/bonus frequency is unavailable: KarmaReward has zero reward
indices/drop chances, and KarmaRewardExponential does not identify actual payout
rows. The implemented fixed/bonus frequency is a fallback, not exact recovered
balance. Gauge shard production is separately based on the native formula.

Conservative configurable validation defaults in BattleRules.json cap reported
clears at 100 waves, require at least one elapsed second per cleared wave, and
cap default shard settlement at 100,000. EndWave must equal ClearWave + 1;
kill totals must match the extracted looping wave counts and partial wave.
These bounds do not verify combat or reproduce original anti-cheat. If owned
flasks are spent or locked during combat, settlement requires making the needed
quantity available again. No in-game playthrough or original payout fidelity is
claimed. Boss shard consumption and infinite-level rewards remain unfinished.

Final validation: all 174 Rust tests pass, including both Karma integration
tests and four Punishment tests. Release build succeeded. All ten reconstructed
client rows parse at runtime, their mappings reference existing stages, and the
installed client patch remains byte-identical when reapplied. A timestamped
pre-Karma DLL backup is retained in the client's Managed directory.

## Native party message restoration

The client archive includes 28,897 files under its Assetbundle directory
(18,763,973,174 bytes), plus Video, PlainAsset, AiScript and table directories.
This inventory is evidence of extensive local assets, not a missing-asset
blocker. Individual references still need validation before declaring every
historical asset present.

PartyManager uses the authenticated message socket for raid/party lobby messages.
`party_messages.rs` now processes native JoinPartyReq/JoinPartyRes/JoinPartyNotice,
PartyDeckHeroInfo, PartyRoomReady, PartyRoomRepeat, ChangePartyRoomInfo,
ChangePartyRoomMaster and LeaveParty. Join acceptance checks a short-lived
pending request, room ownership and capacity, then persists membership before
returning a server-built PartyInfo. The client's subsequent HTTP join is
idempotent. Account identity and hero/equipment snapshots come from server data;
client-supplied recipient lists cannot send party updates outside the room.

Deck selections and readiness persist in battle_state, with ownership, level,
party limits and duplicate checks. Host selections made before any guest joins
are recovered from the join response with server-owned stats. Deck changes
clear readiness. Disconnect clears readiness, while a message-socket reconnect
replays existing members' decks/readiness; complete application-restart party
reconstruction still needs client UI work. HTTP leave/delegate operations send
native removal/host-transfer notifications after commit. The existing room
timeout still governs removal; timeout notifications need further work.

This is multiplayer lobby infrastructure, not a completed combat service.
PartyBattleStart is not forwarded to an unimplemented endpoint. Live combat,
matchmaking/ban-pick, authoritative multiplayer rewards, invitation/private-room
flows, costume changes, battle status messages, and Guild Conquest remain.
Combat entry continues to reject unsupported multiplayer before charging.
No two-Unity-client playthrough is claimed. TCP integration coverage exercises
join, forged identity/stats, host deck recovery, readiness, outsider isolation,
host delegation, leave and notification of kicked players. The full suite passed
175 tests; the extended TCP test also covers the final kick-notification change.

## Client automation verification

The [client automation harness](client-automation.md) is installed and has run
the native client through login, lobby, and Portal UI events. Two concurrent
clients also logged into distinct test accounts. This verifies the automation
setup, not shared combat. A fresh-login Portal smoke run captured a missing
previous World Boss season error; the failure report and screenshot are listed
in the harness documentation. Earlier statements about no native playthrough
refer to full gameplay/combat flows, which remain unverified.
