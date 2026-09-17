# Client integration and supporting services

## Implemented server flows

| Area | Routes / behavior |
| --- | --- |
| Replays | `replay/save_replay`, `get_replay_list`, `get_replay`; persistent native `Info` and opaque LZ4 `BattleLogs`/`Data`, account visibility, bounded storage and duplicate-save detection |
| Recommended decks | `recommend_deck/get_recommend_deck_list`; successful campaign clears, equipment/hero snapshots captured at entry, best total hero level and fastest clear per account, ten of each |
| Honor rankings | Both `records_of_honor/*` routes; stored world-boss, guild-suppression, normal-arena and ban-pick scores, seasons, deterministic ties, own rank, and popup rankers |
| Chat | Session-authenticated native TCP login; owned equipment links for world/channel, whisper and guild messages; persisted link snapshots and history |
| Stamina | `user/get_stamina`, `get_stamina_infos`, `buy_stamina`, `recharge_stamina`; shared battle balances, transactional prices/limits and regeneration |
| Recovery | Login, first-lobby and enter-lobby stamina snapshots; `ServiceBattle` extension and authenticated `battle/recover`; active run and callback result survive process restarts |
| Battle callbacks | Authenticated registration/heartbeat, campaign start/cancel/hero snapshots/results, existing normal-arena match snapshots/results/cancellation, replay sharing and bounded diagnostic reports |

Replays do not execute or verify combat. The native client decompresses and plays the stored logs. Ordinary client uploads are visible only to their uploader and may not nominate other recipients. An authenticated battle service may save a replay for multiple existing participant accounts. Retention is per uploader/owner, including shared replays. List responses omit log data; playback fetches it by UID.

Rankings describe this local server. The world view repeats the local rankings until cross-server aggregation exists. Unsupported modes have empty boards until real scores exist. Historical arena decks come from completed match snapshots; guild-suppression combat and live ban-pick score production remain unavailable. Popup chapter/dungeon metadata is currently zero; original season-to-dungeon mapping is not restored.

## Stamina and configuration

`scripts/export_services_data.py` extracts `ServicesSupport.json` from the supplied client, decoded tables and JIT string pools. `ServicesRules.json` holds local service limits, the local server name, advertised game hosts and authority setting.

- Chicken purchases use the extracted constants: **150 stamina for 50 rubies**. Caller-supplied amounts are ignored.
- Chicken regeneration uses `TeamLevel.RechargeStaminaSec` and the team cap plus owned heroes' `AddStamina`. Fractions of a regeneration interval are preserved. Temporary/pet stamina-cap buffs are not included yet.
- Supported key recharges use `StaminaData` quantities, increasing prices, overflow rules and reset limits. Unsupported/free recharge definitions fail instead of granting arbitrary resources.
- Daily key replenishment continues to use `BattleRules.json`; Sword and guild tickets use the established arena/guild balances. These local defaults may differ from the historical client table. Non-Chicken timed regeneration and original weekly/reset schedules remain to be reconciled.
- Recharge counts reset at UTC midnight and persist between requests/restarts. The dungeon prison-recharge route shares the new native recharge prices and counter.
- `/stamina/buy` and `/stamina/info` are compatibility aliases using native responses. `/stamina/use` requires a positive amount and spends atomically. `/stamina/restore` rejects client grants; restoration comes from inventory, mail and reward transactions.
- Free and paid rubies retain their existing separate balances; stamina snapshots do not overwrite currency fields at login.

`get_stamina_infos` accepts JSON arrays of enum names/numbers and repeated `StaminaTypes` form fields. SessionKey or SessionId is required.

## Required chat client change

The extracted `NShared.NMessage.LoginReq` has no authentication token. The server now requires **AccountId and SessionKey belonging to the same game login**. Account-ID-only login is rejected even if that account is already online.

[client-chat-session.patch](client-chat-session.patch) contains the source changes:

1. Add `SessionKey` to `NShared/NMessage/LoginReq.cs`.
2. Include it in both directions of `JM_NShared_NMessage_LoginReq`.
3. Set it from `SingletonUpdater<WebServiceManager>.instance.Requester.SessionKey` in `NGame2/NSocket/MessageSocketConnector.OnConnected`.

Apply the patch to a buildable client source tree or incorporate equivalent changes into the client's assembly patching workflow. The extracted source and installed game binary have **not** been modified or rebuilt. A fresh game login supplies a new session after a server restart. Native numeric strings are accepted in socket packets.

Equipment links accept at most five entries. Only each `EquipItemInfo.SlotIndex` is used; ownership is checked and all item properties are rebuilt from the database. `MaxEquipLevel` is zero, matching the inspected native equipment-link path. No client-supplied item stats or identity are trusted.

## Battle-service adapter contract

Set `BATTLE_SERVICE_KEY` to a secret of at least 32 characters in the server environment. Without it, every registered `internal/b2g_*` and `internal/b2m_*` route rejects authentication. Callbacks use the HTTP header `X-Battle-Service-Key`; a player SessionKey does not authorize them.

### Campaign lifecycle

1. The authenticated player enters through `campaign/begin_campaign`. The response adds `RunId`; retries return the same run without charging again.
2. The adapter calls `internal/b2g_battle_start` with AccountId and the exact chapter/dungeon/difficulty, plus `X-Battle-Run-Id: <RunId>`. This claims that account's pending run.
3. `internal/b2g_get_hero_info` uses the same headers and account, validates requested heroes against the entry party, and returns the captured hero/equipment snapshot.
4. The service calls `internal/b2g_set_campaign_result` with AccountId, Win, PlayTime, AliveHeroIndices and TotalDamage, using the same run header. The server validates the run and party, calculates the clear star from surviving heroes, and executes its existing reward/progression transaction.
5. Identical result retries return success without paying again; conflicting retries and stale run IDs fail. `FinishedAccountIds` does not authorize rewards for additional players: each participant requires its own entered, claimed run and callback.
6. The player can retrieve the result through `campaign/end_campaign` or `battle/recover`. `ServiceBattle` in login/lobby exposes the same persisted recovery state. This extra field and `/battle/recover` require adapter/client support; they are not original client contracts.

Once claimed, a run cannot be completed by a player-reported result. Set `RequireBattleService: true` in `ServicesRules.json` to require service completion for all newly entered campaign runs and normal-arena results. The default is false so existing local/offline gameplay remains usable. This is **not** global combat validation: awakening trials and other mode-specific result paths retain their existing behavior.

Cancel marks an active claimed campaign run completed without refunding its entry cost. Result receipts persist across restart. The service must retain the run ID or obtain it through an authenticated player's recovery response; it must not infer a run from AccountId alone.

### Normal arena and management

- `b2m_get_match_info` retrieves and claims an existing normal-arena battle selected by MatchUid. It returns the registered player and offline opponent snapshots; it does not create live player matchmaking.
- `b2g_get_match_hero_info` requires AccountId and `X-Battle-Run-Id: arena:<MatchUid>` for a claimed active run.
- `b2g_set_match_result` / `b2g_set_match_cancel` validate AccountId, MatchUid, ArenaType and SeasonIndex. Results share the existing scoring/reward code; identical callback retries are idempotent.
- `b2m_ping` / `b2m_get_gameserver_list` persist bounded registration metadata and return configured game hosts. They do not allocate a combat server or queue simulations.
- `b2m_save_replay` supports service-authorized participant sharing.
- `b2g_server_error_log` and `b2g_set_abuser` store bounded diagnostic evidence; evidence reports do not automatically ban players.

### Still unavailable

A compatible standalone battle-server executable/build and its adapter are absent from this workspace. This implementation does not provide Unity combat simulation, live transport/host transfer, simulation scheduling, integrity verification, Eclipse callbacks, shared raid-HP callbacks or guild-suppression callbacks. Those extracted endpoints return failure explicitly, never fallback success. Campaign hero responses also need full combat buff/stat integration before they can reproduce all native battle effects.

The source patch and adapter contract are supplied for integration; neither a patched Unity playthrough nor native replay playback has been run. Server-side HTTP/TCP tests cannot establish those client behaviors.

## Validation

```text
cargo test --quiet
cargo build --release
python scripts/smoke_services.py
```

Tests cover replay round-trip/privacy/retention, stamina costs and rollback, paid-ruby regression, equipment-link ownership, native socket string IDs and token rejection, score-backed rankings, callback binding and duplicate protection, and active-run recovery across a process restart. The smoke script launches the release server with an isolated temporary database and ports; it never modifies the development database.
