# Portal content verification tracker

Started 2026-09-30. This is the working checklist for sequential native-client
verification and repair. Update the relevant row and append evidence after each
content, then continue to the next. Menu visibility is not gameplay completion.

## Acceptance and evidence

For each content: open its Portal panel, enter the destination, exercise the
available core interaction/battle, verify costs/results/persistence, return and
reopen, inspect client exceptions and server errors, fix and rerun. Multiplayer
requires two actual clients and shared results; an offline replacement does not
count. Missing historical values may be inferred from client code, with the
assumption recorded. Use an isolated copy of the save database.

Statuses: **Queued**, **In progress**, **Partial** (specific checks passed),
**Blocked** (named missing dependency), **Verified** (listed acceptance checks
passed). A locked feature must be tested with eligible test-account progression
before its gameplay can be marked verified. Expand subcontent rows as discovered.

## Work queue

| ID | Category / content | Checks and scope | Status |
| --- | --- | --- | --- |
| P00 | Portal shell | Fresh login, all 23 menu controls, opening and exit, no new exceptions | Verified for shell; individual panels tracked below |
| P01 | Chapter | Chapter selector, stages, difficulty, deck, battle, stars/rewards, repeat/dispatch, party play | Partial; battle/rewards passed, dispatch release fixed; remaining checks below |
| P02a | Hall of Heroes: Gateway to Trial | Entry, combat and rewards | Partial; native lock confirmed, eligible test progression needed |
| P02b | Hall of Heroes: Trial Tower | Entry, floors, combat and rewards | Queued |
| P02c | Hall of Heroes: Ordeal Room | Hero awakening flow and costs | Partial; seal unlock, combat, reward and awakening passed |
| P02d | Hall of Heroes: Purification Room | Purification flow and costs | Partial; single/max purification and awakening passed; price fix retested |
| P03a | Tower of Challenge | Floor entry, combat, rewards and reset | Partial; floors 1–2, rewards, next-floor flow and relogin passed; reset countdown fixed |
| P03b | Tower: Restore Artifacts | Artifact-piece exchange, result, insufficient balance and persistence | Partial; exchange and balance gating passed; reopening pending |
| P04 | Memory Archives | Main story; inspect substory/monthly/memory hero availability, playback and rewards | Partial; main-story cinematic, next replay and exit passed; entry warning open |
| P05 | World Map | Map navigation, node entry and progression | Partial; renders, King's Plains selection and Move passed; map warnings remain |
| P06a | Valance: Identification | Inventory selection, price, result and persistence | Partial; one-item identification, price and result presentation passed; batches pending |
| P06b | Valance: Craft | Recipe/materials/cost/result | Partial; craft, discounted costs, persistence and result exit passed; effect warning open |
| P06c | Valance: Management | Equipment management, awakening/enchanting/tier upgrade | Partial; enhance and first/replacement enchant passed; reforge/tier upgrade pending |
| P07a | Arena: League of Victory | Entry, matchmaking, battle and results | Partial; settlement/return passed, earlier mirror-combat evidence invalidated; distinct opponent ID fixed; replay/live multiplayer open |
| P07b | Arena: League of Honor | Ban/pick, matchmaking, battle and results | Partial; separate Honor ranking and native panel passed; Sword2 entry tickets and live ban/pick/combat remain |
| P07c | Arena: League of Triumph | Ordeal decks, entry and results | Partial; instant-win identity bug fixed; actual attacks, HP loss, nonzero damage and defeat verified; later progression/rewards remain |
| P07d | Arena: other modes | Inspect friendly/World/Lucky Arena availability and dependencies | Queued |
| P08a | Raid: Dragon Raid | Selection, solo/party decks, battle, rewards | Partial; Fire solo combat, loss, victory loot and Hard unlock passed; other dragons/dispatch/live party remain |
| P08b | Raid: Dragon Raid Hard | Selection, solo/party decks, battle, rewards | Partial; 80 definitions reconstructed; Fire solo victory loot, loss and shared Stage 2 unlock passed; other dragons/live party combat remain |
| P08c | Raid: Field Raid | Entry, party behavior, results | Partial; 16 definitions reconstructed; Infira/Imperial/Kibeord Stage 1 victories and loot passed; Black Stage 1 and Infira Stage 2 combat/loss passed; remaining victories/dispatch/live party open; projectile rendering and Chapter 9 return warnings repaired |
| P09 | World Boss | Rotation, previous season, rankings, entry, damage, rewards | Partial native pass: Protianus combat, score/HP/ticket, rank and return verified; reward delivery and visuals remain |
| P10a | God King's Temple: Trial | Trial selection, limits, battle and rewards | Queued |
| P10b | God King's Temple: Soul Weapons | Unlock/upgrade/ether/limits and persistence | Queued |
| P11 | Eclipse | Deck save, entry, waves, team transition, settlement; online branch | Queued |
| P12 | Shakmeh | Inspect lesser/greater branches, stages, decks, gauge, rewards | Queued |
| P13 | Challenge Raid | Rotation, solo/team entry, battle, score and ranking | Queued |
| P14a | Guild: War | Guild membership, signup, matchmaking and battle | Queued |
| P14b | Guild: Conquest | Board, session, party entry, combat and shared progress | Queued |
| P14c | Guild: Raid | Boss entry, shared HP and rewards | Queued |
| P15a | Guild Territory: Headquarters | Building entry/upgrade and persistence | Queued |
| P15b | Guild Territory: Training | Training/skills, cost and effects | Queued |
| P15c | Guild Territory: Rankings | Board and rank consistency | Queued |
| P15d | Guild Territory: other services | Shop, raid/conquest buildings and navigation | Queued |
| P16 | Pets | Pet House, incubation, collection, progression, exploration and bonuses | Queued |
| P17a | Technomagic Raid | Selection, solo/party battle, rewards | Queued |
| P17b | Technomagic Enchantment Raid | Entry, tickets, battle and rewards | Queued |
| P18 | Treasure House | Rotation, selection, entry, battle and settlement | Queued |
| P19a | Apocalypsion: Boss/Punishment | Sub-bosses, affixes, final boss, retry and rewards | Queued |
| P19b | Apocalypsion: Karma | Entry, wave result, flasks/shards and settlement | Queued |
| P20 | Rune Crafting | Recipes, storage/expansion, materials and crafted results | Queued |
| P21a | Adventure Board: Tower | Notice, unlock and destination | Queued |
| P21b | Adventure Board: Stockade | Notice, entry, battle and rewards | Queued |
| P21c | Adventure Board: events | Inspect native notices/rotations and destinations | Queued |
| P22a | Shop: General | Listing, currencies, purchase/limits and persistence | Queued |
| P22b | Shop: May's Ruby | Listing, purchases and costs | Queued |
| P22c | Shop: Stamina | Recharge quantity/cost/reset | Queued |
| P22d | Shop: other currencies | Inspect raid, World Boss, Arena and guild shops | Queued |
| P23a | Orvel: Castle | Town destination and available services | Queued |
| P23b | Orvel: May | Shop and town interactions | Queued |
| P23c | Orvel: Hero's Inn | Friendship/recruitment/roulette | Queued |
| P23d | Orvel: Stockade | Dungeon branches, combat and rewards | Queued |
| P23e | Orvel: Blacksmith | Forge/awakening/enchanting/reforge and costs | Queued |
| P23f | Orvel: Library | Town entry, archive functions | Queued |

Inventory source: client `PortalRenewalCategory`/`PortalRenewalSubCategory`,
recovered `PortalRenewalTable.json`, and installed `RestoredPortal` additions.
Some listed subsidiary services are reached inside a category rather than
having a separate Portal tile. They remain part of verification scope.

## Findings and completed checks

### P00 — Portal shell (verified)

Prior fresh-login smoke test failed with `GetPrevWorldBossSeason failed:
prevWorldBossIndex is null. currentWorldBossIndex=1`. Native `WorldBoss` takes
the first advertised boss, then looks for a predecessor with matching IsGlobal.
The server advertised every table row in index order, including obsolete local
boss 1, whose NextIndex now leads to a global boss and has no predecessor.
Current table contains a closed global rotation 3 -> 5 -> 4 -> 3, plus special
self-loop rows 97/98/99. The response now advertises the current member of the
closed global cycle. `WorldBossRotationStart=3` and existing seven-day season
epoch are documented local scheduling assumptions; the NextIndex links are
recovered client data. Historical boss IDs remain usable by existing records.

Three World Boss regression tests passed. Actual fresh login -> lobby -> Portal
asserted all 23 category controls, waited for async errors, captured the screen,
and exited successfully with no new errors. Evidence:
`target/client-automation/20260930-155141-76a825/report-9df422acfcab4e1abf6c50f14587007d.json`.
This does not verify World Boss combat or every category panel.

### P01 — Chapter (in progress)

Native Portal Chapter -> Move 2-7 -> confirmation -> map -> Prepare Battle ->
deck -> Start Battle successfully entered actual Battle2 combat and won with
three stars using the saved four-hero team. Database confirmed stamina
1,000,761 -> 1,000,754, gold 991,187,619 -> 991,189,880, clear_count=2,
best_star=3 and completed run. Combat generated no new captured errors.
Evidence: `target/client-automation/20260930-155453-7cef23/chapter-before.json`
and victory screenshot `131528b0c3f744b5849ddae6a0f8de70.png` in that run.

Automation repair: native preparation handlers require currentTouch.current;
the helper now provides/restores NGUI event context. Result touch uses a press
event, added separately from click. A saved `chapter-battle.json` scenario is
being validated through reward presentation. No fake result packets are used.

Replay completed and reached native EndBattleReward, team-level-up and reward
popups. Dismissing these and Exit returned to Lobby. Replay evidence:
`target/client-automation/20260930-160209-e4e091/report-d777d20edf334734b60d570fd989c3f9.json`
and `chapter-after.json` in the same folder. Reward/pop-up dismissal was checked
interactively after the saved scenario; it is not yet encoded in that scenario.

Open findings: map navigation logs missing predecessor warnings for inert
chapter-2 nodes 105 and 300. These recovered nodes have Function=None and no
prerequisite; their intended visibility needs reconstruction. Repeat, dispatch,
other difficulties/chapters and party play are not yet verified.

Dispatch UI accepted four heroes, started its native minimum batch of 10, and
returned a cancellation result without new client errors. Evidence:
`target/client-automation/20260930-160209-e4e091/dispatch-cancel.json`.
`ContentUIDispatchBattle.OnFocusOutCount` clamps count to 10..200; entering 1
is not a valid single-dispatch test. Server maximum is currently 100, so the
upper range differs and requires repair. A canceled run still showed heroes
as "Dispatched" after reconnect; availability serialization requires follow-up.
Do not mark dispatch verified until completion/refund/hero-release agree in UI
and database. The helper now submits text fields, but this client also uses
focus-out callbacks; direct text edits alone do not prove the committed count.

Dispatch follow-up: fixed terminal runs being included in active list/login
snapshots. The native manager reserves every listed hero regardless of state.
Terminal records remain stored to reject duplicate settlement requests. Server
maximum now matches the client's 200. Both dispatch regression tests passed,
including empty active lists after collection/cancellation and cancellation
relogin, while preserving refund and duplicate-collection checks.
Fresh native login and reopening dispatch showed no Dispatched labels and no
new errors; selecting four heroes enabled Start Battle. Evidence:
`target/client-automation/20260930-162914-c08639/dispatch-relogin-fixed.json`
and `dispatch-reusable-heroes.json`. Full timed completion, repeat, other
difficulties, map warnings and multiplayer remain open; moving to P02 with P01
explicitly partial.

### P02 — Hall of Heroes (in progress)

Portal presents Gate of Ordeals, Room of Ordeals and Room of Purification.
Gate requires a five-star hero and clearing 4-21; current test save lacks the
chapter clear. Do not count its lock as a completed gameplay test.

Room of Ordeals: selected Frey (two stars), unlocked three seals using native
Use confirmations, entered the Darkness of Frey battle (level 25), won actual
combat, and reached rewards with the Room of Purification destination. No
combat/settlement exceptions. Entry also exposes the existing map-node warning
for chapter-1 nodes 205, 206 and 300. The initially disabled Enter Dungeon is
expected until all seals are unlocked; no bypass was added.

Purification: one Purify cost 30,000 gold and increased progress 0 -> 8%.
Max Purification quoted 12 tries / 360,000 but charged 300,000, revealing that
the server incorrectly rolled great successes in max mode. Native
MaxPurifyPopup explicitly uses PurifyAmount1 with an affordability cap and no
great successes. Server now follows that rule. The awakening lifecycle regression
passes with an added exact deterministic gold-cost assertion. Native retest of
this correction remains pending. Existing flow reached 100%, then Awakening
changed Frey from two to three stars in the isolated database without errors.
Transcendence, low-funds max purification, event discounts and reconnect checks
are not yet verified.

Evidence in `target/client-automation/20260930-162914-c08639/`:
`hall-ordeals-unsealed.json`, `hall-ordeals-combat.json`,
`hall-ordeals-result.json`, `hall-before-purify.json`,
`hall-after-purify.json`, `hall-max-purify.json`, `hall-awakened.json`.

Price-fix retest passed in fresh client run
`target/client-automation/20260930-163831-19c052/`. Its `fixture.json`
documents restoring the observed 8% state, two-star Frey and one trial essence
in a further isolated database copy for replay. Native Max Purification quoted
360,000, charged exactly 360,000 (991,161,678 -> 990,801,678), and reached 100%
without new errors. See `max-purify-quote.json` and `max-purify-fixed.json`.
This supersedes the pending native price retest above. Four automation harness
tests also pass. Remaining P02 checks are explicitly listed above; P03 is next
for broader sequential coverage while those partial checks stay open.

### P03 — Tower of Challenge (started)

Native Portal selection reports `Clear 2-10 to unlock` on the current test
save. No new exceptions; the Chapter panel remains selected while locked.
Evidence: `target/client-automation/20260930-163831-19c052/tower-portal.json`.
Next step is eligible progression in an isolated fixture, then actual floor
combat/reward testing. This is not a gameplay pass.

User cleared 2-10 with three stars in the running automation save (not the root
sprk.db). Preserved it as `before-tower-user-progress.db` in run
`20260930-163831-19c052` before further testing. Tower now unlocks normally.
Selected a four-hero party, won floors 1 and 2 through native combat, received
the reward popups, and used Next Battle to reach the next floor preparation.
Tower 21 saved CompletedFloor=2, keys decreased 5 -> 3, and artifact pieces
increased 1600 -> 1620. No combat/settlement exceptions; map transitions still
produce the previously recorded inert-node warnings. Evidence in that run:
`tower-entry.json`, `tower-floor1-result.json`, `tower-floor2-result.json`.

Found reset timer showing nearly 30 days on September 30: enter-lobby omitted
RemainDaySecond/RemainWeekSecond/RemainMonthSecond. Added countdowns using the
same UTC calendar boundaries as server resets. Boundary test covers month-end,
year-end, leap February and Monday/Sunday. All four Tower tests also passed
(`target/tower-verification-tests.log`). Fresh-login native retest in
`20260930-201347-b722ca/tower-reset-fixed.json` shows 10h45m until October 1,
three keys and 1620 pieces. Actual rollover and high floors remain unverified.
The 0-second entry-key recharge display remains a separate open finding; keys
currently follow reconstructed daily-default rules, not timed recharge.

Restore Artifacts: native exchange consumed exactly 1000 of item 43056,
1620 -> 620, created one equipment item (83 -> 84), and displayed Burning
Brazier of Elf. After dismissal, Restore was disabled below 1000 pieces.
No new errors. Evidence in `20260930-201347-b722ca`: `before-artifact.db`,
`artifact-entry.json`, `artifact-result.json`, `artifact-insufficient.json`.

### P04 — Memory Archives (partial)

Portal exposes Main Story. Native chapter list correctly makes Chapters 1–2
available on this save, with later entries marked Cannot read yet. Chapter 1
opened its replay list, rendered the opening cinematic and subtitles, and
supported Skip -> Next into another replay and Skip -> Exit back to the list.
No new playback exceptions. Visually inspected the rendered cinematic screenshot.
Archive entry logs `Node not found : 4`; navigation/map destination needs repair.
Other archive branches and story rewards are not verified.

Evidence in `target/client-automation/20260930-201347-b722ca/`:
`memory-entry.json`, `memory-playback.json`,
`766bc859778b4b95b7a878ed8af4d78d.png`, `memory-next-playback.json`,
`memory-exit.json`. Release server rebuilt successfully with the lobby countdown
fix. Current run is a database copy preserving the user's 2-10 clear and Tower
progress; root `sprk.db` has not been overwritten.

### P05 — World Map (partial)

Actual map rendered; destination controls are unlabeled collider buttons in
automation snapshots. King's Plains -> Move returned to its chapter map.
No destination-selection errors; chapter entry repeats inert-node warnings
205/206/300. Evidence in run `20260930-201347-b722ca`: `world-map-move.json`
and `f8b1268becdb482bb4e9ad2054ef0771.png`. Other destinations remain untested.

### P06 — Valance (in progress)

Identification opened with the correct no-unidentified-gear state. Craft
selection Galgoria/Knight/Armor quoted 3400 materials and 9,000,000 gold.
Found server ignored the owned-hero 15% discount (hero 111); fixed base,
additional and sub-material calculations using native multiplicative rules.
Created a separate SQLite fixture with exactly 3400 item 990001 for testing;
source save inventory unchanged (`20260930-201347-b722ca/valance-fixture.json`).

Native craft then exposed named enums rejected by the numeric-only parser.
Added client enum names for set, class, part and subpart. Failed request left
gold/materials unchanged. Regression uses native names and exact discounted
balance; all three Valance tests pass (`target/valance-verification-tests.log`).
Native success and identification are the next checks. Initial evidence in
`20260930-204359-dab609/craft-result.json` preserves the rejected request's UI.

Follow-up repairs and native verification:
- Craft succeeded with exactly 3400 materials and 9,000,000 gold, creating
  unidentified armor 911101 in slot 85. The result window initially stayed
  black: restored Portal actions opened town-only cinema windows without their
  owner. Actions without a campaign node now use native inventory cinema entry
  methods; town actions retain their existing entry. Applied via --portal-only
  with timestamped Assembly-CSharp backup.
- Identification sent scalar/repeated form fields, not a JSON array. Valance
  request parsing now retains and normalizes those fields before validation.
  Native one-item identification charged 500,000, saved Identified=1 and extra
  options, presented its animated result and returned normally.
- Craft replay's result screen now finishes and dismisses. One animation effect
  remains invalid: `Effect_ValanceShop_Production_Result1.unity3d` asset
  `6.GameObject.ValanceShop_Production_Result1_Electric______`. Gameplay saved
  correctly; do not count visual assets as fully verified.
- Enhancement used 10 of item 990101 and 1,000,000 gold, changed slot 85 from
  zero to one star, and presented its result with no new errors.
- First enchant incorrectly created a pending choice which the native UI never
  confirms. It now directly applies when no enchant exists. Replacement keeps
  the pending old/new selection. Native first enchant and replacement/new choice
  both passed; each used one 990108 core and 500,000 gold. Confirmed option
  persisted and pending record cleared. Three Valance regressions pass, including
  first-vs-replacement lifecycle, native enum names and discounted cost.

Evidence: `20260930-205320-375f3d/identify-result.json`, `identified-db.json`;
`20260930-213003-965b50/craft-cinema-fixed.json`, `enhance-result.json`,
`enhance-db.json`; `20260930-213530-ccdc0d/enchant-first-fixed.json`,
`enchant-first-db.json`, `enchant-replacement.json`, `enchant-confirmed-db.json`.
All Valance mutations used the documented material fixture, not the user's
source inventory. Reforge, tier upgrade, batch identification and other recipes
remain open; proceed to P07 with those limits recorded.

### P07a — League of Victory (in progress, 2026-10-01)

- Native registration sent scalar/repeated HeroIndices fields and was rejected
  as Invalid Hero. Arena parsing now preserves these fields and validates
  normalized numeric IDs. Actual four-hero deck subsequently entered combat.
- Offline battle settled, but replay creation threw a NullReferenceException:
  MatchedNpcInfo.BattleInfo was null. Matchmaking now snapshots the opponent's
  battle score and tier. Four Arena regressions pass, including snapshot fields,
  duplicate ticket protection and rejected result replay.
- Native retest reached the result screen without the battle-end exception:
  score 1020 -> 1040, win count 1 -> 2, run marked complete. Evidence in
  `20261001-085803-4b9d77/arena-result-fixed.json`, `arena-result-db.json` and
  `81116c7b978e405c90f15bd87d553af5.png`.
- Delayed return to lobby exposed another null dereference:
  MatchManagement.OpenMatchSelect assumes a non-null EntryMenu even when its
  own battle-return caller supplies null. Portal patch supplies the native
  two-slot selector only when this menu is absent; existing menus are retained.
  Applied to backed-up client DLL; native return retest passed.
  Original evidence: `20261001-085803-4b9d77/arena-return-error.json`.
- Replay table was still empty after settlement; replay saving/playback remains
  unverified. Chapter-map warnings 205/206/300 repeat on lobby reload.
- These checks cover the native offline-opponent branch only. Live matchmaking
  and synchronized multiplayer combat are not verified and remain required.
  All matches ran against an isolated copy preserving the user's 2-10 clear.

Return retest evidence in `20261001-090237-3e27fd`:
`arena-result-retest.json` and `arena-return-fixed.json`. Third win reached
1060 points and reopened League of Victory's Ready to Duel screen without
the null-reference exception or return-state timeout. Battle Record opened
without errors but reported no records (`arena-battle-record.json`); replay
storage/playback must still be repaired. Debug and release server builds and
the Portal-only DLL patch completed; Arena regression suite: 4 passed.

### P07b — League of Honor (blocked, 2026-10-01)

Native selector entry opens the Honor panel but get_match_rank fails, displayed
as an unstable-network message. This is a server implementation gap, not evidence
of network instability: `community/arena.rs` rejects nonzero ArenaType before
rank handling. The panel then shows stale Victory information. Do not count that
display as Honor rank support. Evidence:
`20261001-090237-3e27fd/arena-honor-entry.json`.
Needs separate BanPick scores/rank responses and synchronized live ban/pick and
combat; removing the guard alone would wrongly reuse Victory data. Next work:
Honor's mode-specific rank support, then its live flow and remaining Arena tiles.

Follow-up: implemented separate `kind=1` Honor rank queries and snapshots using
the native `BattleBanPickInfo` response field. Exported 36 `GlobalBanPickTier`
rows from the client, using the same keyed schema as MatchTier. Existing Victory
settlement/rewards remain scoped to kind=0; Honor combat is still unavailable.
Starting score 1000 and weekly season timing reuse the documented emulator
defaults; these are assumptions, not recovered historical Honor rules.

Five Arena tests passed, including differing Victory/Honor scores, rank list,
Honor persistence across relogin and rejected unsupported Honor registration.
Native run `20261001-101601-95c61e` opened Honor without the previous error and
displayed its own 1000 points instead of Victory's 1060. Reward information and
Available Heroes opened without exceptions. Evidence: `honor-rank-fixed.json`,
`honor-available-heroes.json`, `6914ce92a5bf40ad84bf821dfa049e9f.png`.
Ready to Duel now reaches the native Sword2 currency gate (balance zero), shown
in `honor-duel-entry.json`. Sword2 replenishment/charging and the live ban/pick
service remain unverified/unimplemented; this is not completed Honor gameplay.

### P07c — League of Triumph (partial, 2026-10-01)

Native Bronze map rendered, but first event returned NotAvailableNodeIndex.
Server incorrectly published OrdealNodeInfos before tier selection; the client's
IsSelectedTier checks whether that list is nonempty and skipped select_tier.
Responses now hide nodes until Selected=true, including preexisting unselected
saved states. Regression verifies empty initial map, selected map, first event,
buff choice, battle proof validation and duplicate settlement rejection: passed.

Native retest `20261001-102251-1be31a` presented Bronze confirmation, accepted
selection and completed the first event without exceptions. Stage became 1/16;
database cleared nodes [1,2], Selected=true and offered three buffs. Event art
and dialogue rendered; Escape advanced to the native buff chooser, and selecting
then confirming a buff was exercised. Evidence: `triumph-tier-selection-fixed.json`,
`triumph-first-event-fixed.json`, `triumph-event-db.json`,
`triumph-buff-confirmed.json`, `triumph-buff-db.json`.
All work used an isolated save copy. Later battles, death/resurrection, opponent
refresh, other event types and final rewards remain to be verified.

First combat follow-up: selected four owned heroes and cleared node 4. Native
result showed +100 rating, Stage 2/16 and another buff selection. Map return and
relaunch retained cleared nodes [1,2,4], score 100 and buffs [7,9]. Evidence in
`20261001-102251-1be31a`: `triumph-battle-entry.json`,
`triumph-combat-result.json`, `triumph-first-battle-return.json`.
Client logged four `Snapshot is not writable.` errors during combat; effects
and snapshot mutation still need investigation despite successful settlement.

Found spendable Ordeal Points were never awarded: settlement returned only the
rating/PointResult, while native results consume CurrencyResult. Winning battles
now grant OrdealArenaPoint transactionally and return that currency result.
Base amount uses the existing configurable 100-point win rule (an emulator
assumption); HP/time/survival bonuses are not implemented by this change.
Regression verifies awarded balance and that duplicate settlement cannot grant
it twice (`target/triumph-currency-tests.log`: one test passed). Native payout
retest proceeds from the next node in isolated run `20261001-102952-c389cc`.

Native payout retest passed: node 8 victory displayed Obtained Points 100 and
score 200 (+100). SQLite held OrdealArenaPoint=100 and cleared nodes [1,2,4,8].
After selecting the next buff and exiting, the map displayed Stage 3/16, score
200 and Ordeal Points 100. Evidence in `20261001-102952-c389cc`:
`triumph-currency-result.json`, `triumph-currency-db.json`,
`triumph-currency-return.json`, `42ed9b01105f469396dba174285bbd19.png`.
Debug/release binaries rebuilt. Snapshot-writability warnings repeated (eight
with two buffs); their cause and actual buff effects remain unverified. No
historical payouts were backfilled into the source save.

Correction following user report: previous victories did not verify fighting.
The one-account fallback copied the local account ID into the opponent. Native
BattlePlayerContainer rejects duplicate IDs; PlayerEnterBattleLog then skips
enemy creature creation and ArenaCondition sees an empty enemy team. Earlier
zero-DPS result screens should not have been treated as combat success.
Mirror opponents now use a negative, battle-only ID; no real account is created
or modified. Existing Triumph maps repair matching IDs on read without resetting
progress. Normal Arena's new offline mirror matches receive the same fix.
Regression covers distinct IDs and saved-map repair with progress retained;
native combat retest is pending.

Native combat retest passed in `20261001-104200-90e157`: both teams spawned
four heroes (`2a8f97956df449859d2b2c66d7430023.png`), followed by visible attacks,
falling HP and nonzero damage totals (`557f65ba4cce4e29a7a922021006779c.png`).
At 1:16 remaining the enemy still had 85% HP and the local team was defeated.
Settlement correctly kept the score at 300 and currency at 200, did not clear
the attempted node, cleared ActiveNode, and marked the four party heroes dead.
Evidence: `triumph-active-combat.json`, `triumph-real-combat-result.json`,
`triumph-real-combat-db.json`, `32f3b8ecee4f4959bcd88ae5e464ea6a.png`.
No client errors were captured during this battle, including no snapshot warning.
Earlier Victory mirror wins are also settlement-only evidence; its fallback
received the same identity correction, with a separate single-account regression.
The live server runs the repaired debug build; the release binary was rebuilt.
This proves actual combat and loss handling, not the balance of copied decks,
all buff effects, full Triumph completion, or live multiplayer restoration.

### P08a ? Dragon Raid (partial, 2026-10-01)

Portal lists Fire/Frost/Poison/Black Dragon. Fire Dragon T6 Stage 1 party room,
six-hero selection and start confirmation worked; start is blocked by the
server multiplayer guard. No party combat success is claimed.
The separate native Single mode was incorrectly blocked too: its CampaignDungeon
row uses BattleType=12, shared with multiplayer raids. Validation now permits
only Raid.Type=1 with IsOnlineSingle=false through the campaign lifecycle;
raid identity, hero level and party-combat guards remain intact.

Regression passed: rejects under-level heroes and party route, permits eligible
solo entry, charges 48 stamina once on retries and accepts loss settlement.
Built debug/release. Evidence before fix: run `20261001-145046-72cf9e`,
`dragon-battle-start.json`, `dragon-solo-start.json`.

Native retest used a separate fixture raising six heroes to level 60 (required
by client raid data), documented in `20261001-145046-72cf9e/dragon-level-fixture.json`.
Source save levels were untouched. Run `20261001-145918-4ff1c9` spawned all six
heroes and Fire Dragon; attacks, damage totals and boss HP reduction were visible
(`a28481bc1af6446e825be2328539dae7.png`, `dragon-solo-combat.json`). The party lost
with its existing gear. Run completed, stamina 1000719 -> 1000671, and lobby
return had no client errors. Evidence: `dragon-solo-result.json`,
`dragon-solo-db.json`, `dragon-solo-return.json`.
Victory loot, other dragons, dispatch/repeat, Hard/Field and synchronized party
combat remain unverified. Do not use this level fixture as the user's main save.

### P08b/P08c ? Hard Dragon and Field Raid (entry audit)

Native shortcuts open their lists without exceptions, but neither contains any
raid entries. Extracted Raid rows contain types 0/1 (Dragon Normal), 7/9
(Challenge); no types 2/3 (Hard Dragon) or 12/13 (Field). These categories need
reconstructed definitions linked to suitable client dungeon/wave assets; the
empty menus do not verify gameplay. Existing Punishment/Karma overlays do not
supply these categories. Evidence in `20261001-145918-4ff1c9`:
`dragon-hard-entry.json`, `f0572e403fc843dea3f76aac914f4dc4.png`,
`field-raid-entry.json`, `5dfdf04035b04706a1a30607788368f6.png`.
Next: audit the available Hard/Field dungeon and wave records and reconstruct
matching raid entries with documented assumptions, then test combat.

### P08b - Hard Dragon reconstruction and combat (2026-10-01)

Added 80 shared client/server definitions: four dragons, ten stages, party and
native solo variants. All point to retained CampaignDungeon and populated wave
records (party chapters 911-914; solo 931-934). Combat rules, monsters, levels,
stamina and dungeon drops come from those native records. The client overlay now
also supplies stage labels, unlock conditions, solo links and reverse party links.

Assumptions: IDs 11-14/111-114; required hero level 70; unlock when the matching
Normal raid has stage 8 selectable; labels `Hard Stage N`; three players with
three heroes each for party, four main/four sub heroes for solo. Extra individual
rewards, raid points and gem resets are zero because their Hard definitions are
missing. Existing dungeon drop rewards remain. Generator:
`scripts/reconstruct_hard_dragons.py <client TableJit directory>`.

Native run `20261001-152946-e0b22f`: list displays all four dragons and portraits
(`7c565f1ac66e4c148263108e73548c73.png`). Fire Stage 1 solo team setup supports
four main/four sub slots; six owned heroes entered. Dragon attacks, nonzero hero
damage, falling boss HP and hero deaths were visible
(`70e3bb5e0db14ad5b89b40d522b0220e.png`, `hard-dragon-active-combat.json`).
The team lost; the run completed, stamina fell 1000671 -> 1000611, and return to
Lobby captured no client exceptions. Evidence: `hard-dragon-result.json`,
`hard-dragon-db.json`, `hard-dragon-return.json`.

This used a separate level-70/unlock fixture, described in
`20261001-145918-4ff1c9/hard-dragon-fixture.json`; it is not user progression.
Do not claim a victory, all stages, other dragons or multiplayer were played.

Progression fix: native RaidHelper reads the shared raid index and interprets
RaidLevel as highest selectable stage. Normal/Hard solo victories now return
that shared index, unlock the next existing stage, and preserve higher progress
when replaying easier stages. Hard entry checks unlock, selected stage, hero level
and dungeon identity; party entry stays guarded. Regression tests cover these
checks, single stamina charge, loss, victory/replayed settlement and progression.
Victory tests exercise server settlement, not native combat balance or loot UI.

Validation: all three `cargo test dragon_solo` regressions passed; release build
passed (`target/hard-dragon-tests.log`, `target/hard-dragon-release.log`). All 80
definitions were checked against retained populated native wave rows. Client DLL
was backed up and patched with the shared overlay. The level/unlock fixture was
stopped after testing; resume from the non-fixture `20261001-104200-90e157` save.

Next queued content: P08c Field Raid definitions and native entry/combat audit.

### P08c - Field Raid reconstruction and native combat (2026-10-01)

Added 16 shared definitions for Infira, Imperial Army, Kibeord and the Black
Knight encounter: two stages each, paired party/solo routes. Dungeon pairs are
8:100/101 and 9:100/103, 101/104, 102/105; stage 2 adds ten to each dungeon ID.
All sixteen have native populated waves. BattleDefineCode identifies the solo
and party routes; creatures, battlefield, wave levels, stamina (54/70), squad
capacity and dungeon drops remain native. Generator: `scripts/reconstruct_field_raids.py`.

Assumptions: IDs 21-24/121-124, chapter-8 hero minimum 80/chapter-9 minimum 90,
unlock after 8-26 or 9-23 respectively, two party players, stage labels, and no
extra individual reward/raid points/reset gem fee. The generic Imperial Army
name is reconstructed; other names resolve through native creature localization.
Party allocation has not been verified in live multiplayer.

`EnableLegacyFieldRaids` allows only matching open Type=13 solo raid definitions
through the closed chapter-8/9 check. Story dungeons stay closed; chapter entry
prerequisites, raid unlock, hero level, stage progression and identity still apply.
Party definitions cannot use offline campaign combat. Solo wins update the shared
raid index and unlock the next existing stage without downgrading progress.

Native testing found unlocks were invisible after login when progress existed
only in battle_state. Login now appends those records, matching lobby refresh.
The native Easy unlock check means the dungeon's minimum available difficulty;
these story stages use Normal, so server checks and fixture use Normal clears.
Regression covers reconnect, disabled rule, closed story route, locked stage,
party/mismatched raid, under-level hero, one-time charging, win progression and loss.
It passed in `target/field-raid-relogin-tests.log`; debug/release builds passed.

Native run `20261001-161226-d7c98d` displayed all four unlocked encounters with
portraits (`53c5c2a65cd0425e98afc6d9a5409d4a.png`). Infira Stage 1 solo accepts
four heroes, enters the native three-wave battlefield and fights wave 1:
`777a372b32c4498994cad7b71f207e91.png` shows attacks/damage/hero death;
`01d2f7c1d65e48c5b572b6c45ecaba06.png` shows subsequent damage and falling HP.
The weakly equipped fixture team lost both attempts in wave 1. Boss wave and
victory are NOT verified. Stamina 1000719 -> 1000611 matches two charges of 54;
the run is completed. Evidence: `field-infira-second-3.json`,
`field-infira-second-5.json`, `field-infira-second-result.json`, `field-infira-db.json`.
No combat exceptions. Exit returns Lobby without errors (`field-infira-return.json`).
Change Party reopened the deck but emitted `Node not found : 4` on lobby load;
that map warning remains open. Direct Retry is disabled by native battle-menu/
retry-condition rules; a new battle through Change Party worked.

Fixture provenance: copy of non-fixture `20261001-153929-8902ae/sprk.db`, with
level-90 heroes and only the prerequisite clears (7-12, 8-26, 9-23) added.
Initial fixture was in `field-raid-fixture.db/json` in that directory; the copy
in `20261001-160758-c51e31` corrected those clears to Normal (MaxStar=13,
FirstRewardedDiff=2) before the successful run. These are test saves, not user progress.

Remaining: other three encounters, later waves, native victory/drop UI, dispatch,
the Change Party map warning and synchronized multiplayer. Next queued category:
P09 World Boss; P08 remains partial with these explicit follow-ups.

Broader verification: all 36 `api::battle::tests` passed after the login/Field
changes (`target/field-raid-battle-regressions.log`). Test client/server stopped;
the updated release is relaunched from the non-fixture `20261001-153929-8902ae`
save, preserving the user's original hero levels and campaign progression.


### GM victory retests - P08a (2026-10-01)

User requested upgraded teams for victory verification. `gm.py maxheroes
--source-db <save> --output-db <new save>` now makes a separate SQLite backup,
sets owned heroes to native level 100/star 5/T5 and skill level 91/extension 3,
selects native recommended perks within the point budget, and equips five-star
UW/UT plus compatible T8 dragon armor/accessory/secondary/orb with legal max
option rolls. Existing artifacts are retained; soul weapons/runes are not
synthesized. This is max core progression, not every optional growth system.
Source saves and gm_config.json remain unchanged; output must not already exist.
Fixture provenance: `20261001-161859-a98b7c/max-raid-team.fixture.json`.

Native run `20261001-162832-4f70bd`: Normal Fire solo 905/7 (Raid101/7)
won with real combat (`d4698d212ede4080a5fc866019648b52.png`), victory
(`527d8a0c53f74de484e7caad4e6d502b.png`) and visible loot
(`3d7c5f6384cf4aab929870c4f16a2e32.png`). Eight equipment instances were
added, plus item 91292 x1 and 43077 x5. Stamina 1000719 -> 1000671;
gold unchanged. `gm-normal-fire-reward-delta.json` records exact item IDs.
Shared RaidIndex1 advanced to RaidLevel8, and native Hard Fire entry unlocked
without a manual raid-progress edit (`gm-hard-unlocked.json`). Reward popup,
return to team setup and lobby succeeded without client exceptions.
Evidence: `gm-normal-fire-victory.json`, `gm-normal-fire-rewards.json`,
`gm-normal-fire-return.json`, `before-normal-fire.db`, `before-hard-fire.db`.


### GM victory retest - P08b Hard Fire (2026-10-01)

The initial core-max fixture still lost Hard Fire. `gm.py maxheroes` was extended
with native A2/20 soul weapons (balanced 500/500 stat split). New fixture:
`20261001-162832-4f70bd/max-soul-team.fixture.json`; source save remains untouched.
Native run `20261001-163844-986472` won Hard Fire Stage 1 (931/1, Raid111/1)
with surviving Frey and Roi; result DPS records real fighting and hero deaths
(`5d26d5cbff7d449b8e46794e962e3452.png`). Loot popup
`a383c8f63e384e96837f61ce36a00d96.png` matches item 91292 x1 and 45025 x1,
with no equipment-instance drop on this run. Stamina 1000611 -> 1000551.
Shared RaidIndex11 advanced to RaidLevel2. Victory/reward/return snapshots
captured no exceptions. Evidence: `gm-hard-fire-victory.json`,
`gm-hard-fire-rewards.json`, `gm-hard-fire-reward-delta.json`,
`gm-hard-fire-return.json`, `before-hard-fire.db`, `after-hard-fire.db`.
This verifies first-stage victory settlement, not every stage or multiplayer.


### GM victory retest - P08c Infira and missing loot fix (2026-10-01)

The A2/20 fixture cleared all three Infira waves in run
`20261001-164950-4bec2a`, exposing empty victory rewards. The retained dungeon
DropReward 961000 is empty; reconstructed Field Raid entries had no
IndividualReward. All 16 solo/party Field Raid entries now link the retained
Infira/Chapter 9 reward bundles in `tables/ReconstructedRaids.json`.
The original Raid rows are absent: these links are inferred from bundle names,
stage ordering and the smaller solo quantities, not recovered original mappings.
The client DLL was repatched with the same overlay.

Replay `20261001-173201-562fbf` uses a copy of `before-infira.db` from the
first run. This fixture includes GM story unlocks and is not the user's save.
Infira Stage 1 (8/101, Raid121/1) now passes native three-wave combat,
boss damage, victory, loot popup and return to party setup. Evidence:
- Wave 2: `7843d4d76e4044cb8eb9b7dbbe39feae.png`.
- Boss damage: `118cf12e3a3a466fbff083ab76fde69d.png`.
- Victory: `8eefcc05a0174832b2745fa4f5fabeea.png`.
- Loot: `3e1645d853824b30a38e68dae747b8dc.png`.
- Saved settlement: `gm-infira-fixed-reward-delta.json`, `before-infira.db`,
  `after-infira.db`; four equipment instances (804010, 809010, 805005, 804004),
  item 43077 x15, stamina 1000551 -> 1000497, gold unchanged.
- Shared RaidIndex21 advanced to RaidLevel2; `gm-infira-fixed-return.json`.

The existing Field Raid regression now checks 4-5 equipment drops from the
retained solo bundle and rejects duplicate settlement without granting loot
again. It passed (`target/field-raid-loot-tests.log`).
Native snapshots retain startup table/certificate/Steam errors and the map
`skip node open effect from 101 to 1` / `startNodeIndex 101 depth0` diagnostics.
The latter originate from the client's already-open node bypass logic in this
GM-unlocked fixture. No new combat exception was observed; these diagnostics
are not reported as resolved.

Remaining P08c: Stage 2 combat, Imperial/Kibeord/Black encounters, dispatch,
Change Party map warning and synchronized multiplayer. Party reward links are
populated but native multiplayer settlement is not verified.

After verification, normal-save run `20261001-173837-b69de1` was launched from
`20261001-161859-a98b7c/sprk.db` and passed login to lobby. Heroes and campaign
progress match that normal source exactly; GM fixture upgrades remain isolated.


### P08c Imperial Army Stage 1 (2026-10-01)

Native run `20261001-174000-7d1282`, isolated A2/20 fixture: six-hero
main/sub deck, real three-wave combat and victory passed. Combat evidence
`ede3e1df31ab4e779ea5c78a0252b0e1.png`; victory/loot/return snapshots
`imperial-victory.json`, `imperial-loot.json`, `imperial-return.json`.
`imperial-delta.json` confirms five equipment instances, item 43078 x15,
raid points +7000, stamina -54, gold unchanged, RaidIndex22 Stage 2 unlocked.
No battle errors before settlement; result retains already-open map-node
diagnostics. Stage 2 and synchronized multiplayer remain unverified.


### P08c Kibeord Stage 1 (2026-10-01)

Same isolated run `20261001-174000-7d1282`: six-hero combat, all three waves,
boss HP reduction, victory, loot and return to party setup passed.
`kibeord-combat.json`, `kibeord-pending.json` (boss fight),
`kibeord-victory.json`, `kibeord-loot.json`, `kibeord-return.json` record the run.
`kibeord-delta.json` confirms five equipment instances, item 43078 x15,
raid points +7000, stamina -54, gold unchanged, RaidIndex23 Stage 2 unlocked.
Stage 2 and multiplayer remain unverified. Automation initially pressed the
victory overlay before its animation completed; waiting and pressing again
opened the loot popup, and native close/exit worked.


### P08c Black Stage 1 (2026-10-01, partial)

Run `20261001-174000-7d1282` accepted the six-hero main/sub deck and entered
real wave-1 combat (`155fc780a0bf4a028b2b6c054aed6d71.png`). The A2/20
fixture lost in wave 1; victory/rewards remain unverified. No client errors
were captured during battle. `black-loss.json` and `black-loss-delta.json`
confirm stamina -54, no equipment/currency reward and no RaidIndex24 unlock.
The combat screenshot contains a magenta effect, requiring a shader/material
investigation; asset presence alone does not establish correct rendering.
`black-return.json` records exiting the loss screen.
Kibeord return also retains `Node not found : 26`; its reward success does not
resolve that map-navigation warning.


### P08c Infira Stage 2 (2026-10-01, partial)

Run `20261001-174000-7d1282`: native Stage 2 selection and battle entry
passed. A stale deck toggle in the temporary controller initially left two
heroes selected; the native insufficient-party prompt was cancelled, and
Kasel/Frey/Cleo/Roi entered as four heroes. No incomplete-deck battle was run.
`infira2-setup-corrected.json`, `infira2-combat.json`, `infira2-loss.json`
and `infira2-loss-delta.json` record real combat followed by defeat.
Stamina -70, no equipment or raid-point rewards, Stage 2 progress retained.
This team does not establish victory reward verification. No combat errors
were captured. A stronger/specialized formation is still required for victory.


Server-only follow-up: `all_field_raid_stages_settle_once_and_cap_shared_progress`
passed (`target/all-field-raid-stages-tests.log`). For all four encounters,
Stage 1 -> Stage 2 -> Stage 1 replays check exact stamina charges, idempotent
entry, equipment and raid-point rewards, duplicate settlement rejection, and
shared progression remaining at Stage 2. These synthetic endpoint results do
not replace native battle wins or prove multiplayer combat.
Map warning investigation located the native `NodeGraph` missing-node fallback
and campaign unlock-effect traversal; no global suppression or unverified
navigation patch was applied. The Black magenta effect produced no shader or
material diagnostics in this run's client log; its cause remains undetermined.

Normal-save run `20261001-190419-ec7380` restored from `20261001-173837-b69de1/sprk.db`,
passed login-to-lobby, and matches source hero/campaign records. The GM test
save remains isolated in `20261001-174000-7d1282`.


### P08c rune fixture and story-map response correction (2026-10-01)

`gm.py maxheroes` now fills native rune page 1 with five compatible runes per
owned hero, selected by retained item grade and slot restrictions. Knights and
priests use weapon HP runes; other classes use attack runes; armor/secondary
slots use HP runes. Other rune pages and existing artifacts are retained.
`target/field-rune-team.fixture.json` records the exact items and provenance;
all 30 slots were checked against the native SlotTypes. Source saves stay read-only.
Native run `20261001-200018-69c43e` still lost Infira Stage 2 with
Kasel/Frey/Cleo/Roi (`infira2-runes-loss.json`); the stronger fixture alone does
not establish a victory or resolve the formation requirement.

Root cause of story-unlock diagnostics: EndCampaign iterates CampaignResults
and invokes CampaignNodeOpenEffectManager for legacy Field Raid map nodes.
The server now omits those story notifications only for a matching solo Field
Raid definition (Type 13, matching chapter/dungeon). Dungeon records still save;
CompletedRaidInfo remains the stage-unlock response. Normal campaign responses
are unchanged. All 37 battle tests pass in `target/field-map-result-tests.log`,
including all eight Field Raid stages, reward settlement and Stage 2 caps.
Native return-flow verification follows; `Node not found` is not yet claimed fixed.


Native map-response retest: release build passed (`target/field-map-result-build.log`).
Run `20261001-200630-eefc7f`, Imperial Army Stage 1: full victory, loot,
return to raid deck and lobby passed. `imperial-mapfix-victory.json`,
`imperial-mapfix-loot.json`, `imperial-mapfix-return.json` capture no errors
after clearing startup diagnostics. `imperial-mapfix-delta.json` confirms five
equipment drops, 43078 x15, raid points +7000, stamina -54 and Stage 2 retained.
This resolves the spurious story unlock effects on the verified Imperial route;
Kibeord's prior `Node not found : 26` still needs its own return-flow retest.


### P08c Black projectile material repair (2026-10-01)

Read-only `renderers` automation diagnosed visible null material slots on
`Effect_Monster_Guardian_RoettenBoulderGolem_Attack1_Projectile_DarkAura`.
The three retained projectile bundles (OriginalSpec/MidSpec/LowSpec) contain
the renderer but both serialized material references are zero. This is a broken
reference, not absence of the projectile asset. `scripts/repair_field_raid_effect.py`
assigns the same bundle's retained Trail material to those null slots only.
This is an explicit visual approximation, not a recovered DarkAura material.
Original `.before-field-material` backups are retained beside each bundle.
The script reload-checks output and a second run changed zero renderers.

Native run `20261001-220216-1816e5`: `black-fixed-materials.json` confirms all
18 sampled projectile slots use supported `Vespa/Effect2/NormalNoZ`; screenshot
`165ecb30578d4db1b20ce3ef49568596.png` shows the same wave-1 combat without
magenta blocks. Damage/skills continue normally. Victory remains unverified.
The previous rune-equipped Black retry also lost (`20261001-200630-eefc7f`,
`black-runes-result.json`); the visual repair does not alter battle strength.


Kibeord map-return retest passed in `20261001-220216-1816e5`:
`kibeord-mapfix-victory.json`, `kibeord-mapfix-loot.json`,
`kibeord-mapfix-return.json`, `kibeord-mapfix-lobby.json` capture no errors
through victory, loot, deck return and lobby. The previous `Node not found : 26`
did not recur. Rewards: five equipment pieces, 43078 x15, raid points +7000,
stamina -54; Stage 2 retained (`kibeord-mapfix-delta.json`).
The closed-map story notifications are now fixed on both verified Chapter 9 routes.
Black's post-material-repair attempt still lost (`black-fixed-result.json`);
rendering is repaired, native victory/rewards remain open.
Remaining: winning Black/Infira Stage 2 formations, other Stage 2 native wins,
dispatch and synchronized multiplayer. Server-only reward tests are not native wins.

Updated release restored normal-save run `20261001-220652-452449`; login-to-lobby
passed and hero/campaign records match normal source `20261001-190419-ec7380`.
GM/rune changes remain isolated from the normal save.


### P09 World Boss first-entry repair (2026-10-01)

Isolated upgraded fixture run `20261001-224227-1aff1f` reproduced
`Get world boss ranker failed.` after Portal entry. The native management code
requires a non-null personal rank record even before any battle. The server now
returns the real account identity with Score 0/Rank -1 for an unranked World Boss
player. World Boss response ranks are zero-based because the client displays
Rank + 1; internal one-based reward settlement is unchanged. No fabricated
leaderboard participants are inserted.
Four World Boss tests passed (`target/world-boss-entry-tests.log`), covering
rotation, first-entry/unranked and ranked responses, score/HP/ticket idempotency,
and prior-day/season mail rewards. Native panel/combat retest follows.


P09 native entry retest `20261001-224843-a40e8c`: unranked Protianus panel and
six-hero main/sub deck opened successfully without entry errors. Real combat
`33d0d92d4b0d40ebbeb7b6b655cec999.png` showed damage and falling boss HP.
Global ticket key 18 decreased from daily 2 to 1; legacy world_boss_ticket is
not the global ticket. The result failed with WorldBossHpError; no score saved.
The client CreateEndCampaignRequest leaves TotalDamage unset; its serialized
enemy slot-zero ResultCreatureInfo carries received damage in GivedDamage.
Server fallback now validates a unique enemy slot-zero record and nonnegative
numeric/string damage, retains damage-rate limits and one-time settlement.
Five World Boss regressions passed before additional malformed-record cases.
Particle warnings `SubEmitter ParticleSystem is null` appeared during combat;
these remain open and are separate from score settlement.


P09 follow-up: replay `20261001-230452-1ed3c2` still returned
WorldBossHpError at native settlement; fallback-only repair is not verified.
The end response now also supplies WorldBossRankInfo (Score and zero-based Rank)
because AccountManager applies that response to the native result screen.
All five World Boss regressions pass (`target/world-boss-settlement-tests.log`),
including malformed records and duplicate submission. Native payload diagnostic
run `20261001-231416-238e8b` is investigating the remaining score mismatch.

Native diagnostic `20261001-232459-44d2c5` confirmed CreatureInfoString retains
one URL-encoding layer after form decoding. Boss 5502 reported GivedDamage
83,898,412,126 and Hp 916,101,587,874 (sum = starting 1 trillion HP).
The parser now accepts plain JSON or exactly one additional URL layer, matching
WebServiceRequester.EscapeURL -> WWWForm. Temporary payload logging removed.
Five regressions pass with the native double-encoded form
(`target/world-boss-wire-tests.log`). Final native settlement replay follows.


### P09 native score settlement verified (2026-10-01)

Release run `20261001-232917-9ca41f`, isolated upgraded six-hero fixture:
- Protianus (boss 4, season 39) fought normally; the native result submitted
  74,169,523,329 damage. No WorldBossHpError occurred.
- Result UI displayed Rank 1 and Total DMG 74,169,523,329; Exit returned to
  the leaderboard with the same score. Evidence: `world-boss-score-screen.json`,
  `16dfd4a45ebb41868c4a2b4d7d76906c.png`, `world-boss-return.json`.
- DB confirms score 74,169,523,329, boss HP 925,830,476,671, global ticket key
  18 Count 1. `world-boss-settlement-evidence.json` records these values.
- Previous-season empty ranking and current ranking switch successfully;
  Ranking Reward opens (`world-boss-previous-season.json`,
  `world-boss-current-ranking.json`, `world-boss-ranking-rewards.json`).
- Release build passed (`target/world-boss-wire-build.log`); all 39 battle
  regressions passed (`target/world-boss-battle-regression.log`).

Remaining P09 work: native daily/season mailbox reward delivery and amounts
  (global reward-table parity still needs checking), other rotated bosses,
  cross-account shared score/HP behavior, particle sub-emitter warnings,
  blank final-column reward icons, and Chapter unlock toasts after returning
  from the GM-unlocked test fixture. Reward preview is not proof of delivery.
This verifies the native solo World Boss submission path, not synchronized
multiplayer or full P09 completion. P10a God King's Temple remains queued.

Updated release restored normal-save run `20261001-233413-e93063`; heroes and campaign
progress match source `20261001-220652-452449`. Upgraded fixtures stayed isolated.


### Chapter 1 Clause recruitment recovery (2026-10-01)

Client EventTriggerTable event 10034 runs tutorial 20001 on EnterLobby after
1-20 is complete and before 2-1 is complete. Tutorial 17001 precedes it;
20001 sequence 9 grants reward 81006, HERO_CLAUSE (item/hero 15), 2-star level 20.
Lobby entry and login now recover tutorial 20001 using its existing atomic
completion receipt and client reward data. Catch-up deliberately allows players
already past 2-1. It requires a real 1-20 clear (unlock alone is insufficient),
and preserves owned heroes/upgrades without duplicate recruitment EXP.
The normal save `20261001-233413-e93063` had cleared 1-20 with no Clause or
20001 receipt before this repair. Native login verification follows.

Clause recovery verified in native normal-save run `20261001-235055-8c9d80`: roster
shows Clause level 20 with 2 stars (`clause-roster.json`,
`c993c5a70adc4a4d8f1091db160d651a.png`). Tutorial 20001 receipt persisted;
existing heroes and campaign progress unchanged (`clause-recovery-evidence.json`).
All 17 tutorial tests passed (`target/clause-tests.log`) and release build passed
(`target/clause-release-build.log`). Pre-recovery backup is retained in
`20261001-233413-e93063/before-clause-recovery.db`.


### Fallen Frey / Dark Lord Kasel shop repair (2026-10-01)

Native PayShopManagement.RequestBuyHero chooses the Hero item by creature index.
Dark Lord Kasel (84) uses 1008410; Fallen Frey (85) uses 1008510. Both client
items grant level 50, 5-star, Transcendence 5. The generic validator incorrectly
required Transcendence 0 and the creature default level (0 normalized to 1),
producing InvalidItemIndex before any charge. These exact item/hero pairs now
validate their native recruitment stats; other item variants remain restricted.
Price stays 6000 rubies and mileage 600 per hero, from CreatureStarPrice data.
All 19 hero/shop tests pass (`target/fallen-hero-shop-tests.log`), including
both special heroes, wrong item/price rejection, duplicates and exact balances.
Neither hero was purchased on the user's save during this verification.

Release build passed (`target/fallen-hero-shop-build.log`); updated owned server
restarted on the same normal-save database, health HTTP 200. Client retry still
needed (the user relaunched without the automation bridge); no native purchase
is claimed. Backup: `20261001-235055-8c9d80/before-fallen-shop-update.db`.


### User-requested hero upgrades (2026-10-02)

User explicitly requested gm.py upgrades on the active save. Ran maxheroes
against a stopped-server backup of `20261001-235055-8c9d80/sprk.db`, verified
all 15 owned heroes at level 100 / 5-star / T5 / skill levels 91, and applied
the resulting database to the active save. The script also sets skill extensions,
recommended perks, 5-star UW/UT, T8 dragon gear, A2/20 soul weapons and runes;
existing artifacts are retained. Campaign progress and user_info balances were
compared and unchanged. This is now the user's intentionally upgraded save,
not an isolated raid-test fixture. Original backup:
`20261001-235055-8c9d80/before-user-maxheroes-20261002.db`.


### Shop starting progression override (2026-10-02)

At user request, future ruby purchases of Fallen Frey / Dark Lord Kasel now
accept the native item IDs but recruit at level 1 (minimum playable level),
5-star, T0. Recruitment team EXP uses the T0 star row. This shop-only policy
does not change the original item metadata, other reward sources, or already
owned/GM-upgraded heroes. Price and mileage remain 6000/600. All 19 hero/shop
tests passed (`target/fallen-base-shop-tests.log`).
