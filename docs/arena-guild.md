# Arena and guild systems

`src/api/community` implements the native HTTP guild contracts and local arena/guild-raid flows. It replaces the previous basic guild handlers while retaining their route aliases. Changes use the same SQLite transaction as inventory and currency operations.

## Supported flows

| Area | Behavior |
| --- | --- |
| Normal arena | Register a party, cancel, request an offline opponent, submit a result once, retrieve the saved result, query ranks and local seasons. Opponents use another account's owned heroes; a solo server uses a copy of the player's party pool. |
| Arena rewards | Daily participation records and finished seasons produce reward mail at the next login/lobby visit. Claim ledgers prevent duplicate mail. Daily rank uses the player's last completed match that day. |
| Guild management | Name validation, creation fees, search, free/application joins, accept/reject, member/admin/master permissions, notices/settings, master transfer, kicks, withdrawal and disbanding. Database roles retain their previous values and convert to the native client enum in responses. |
| Guild progression | Daily ruby contributions and threshold bonuses; attendance and reward mail; level and building costs/investments; skill upgrades, requests and refundable resets. Skill resets refund recorded spending. |
| Guild shops | Building 3 unlocks shop 4; its level restricts stock groups. Guild Point balances, native purchase responses, prices, inventory capacity and purchase limits share a transaction. Shop cost type 17 supports Guild Arena Points. |
| Guild raids | Solo campaign entry for legacy guild-raid chapters, weekly shared boss HP, bounded reported damage, member totals, advancement, participant kill-reward mail, shared expiring loot stock and purchases. Completed chapters have clear-time rankings. |
| Guild arena | Registration/auto-registration, up to three defense decks, pairing, cached enemy decks, offline attacks, deck locks, used heroes, attack records, rankings, historical session records and season reward mail. |

Login/lobby responses restore guild membership, normal-arena battle info, Guild Points and raid/arena tickets. Guild Arena Points use the existing `PlayerCurrencyInfos` storage. Guild chat continues to use the same membership tables.

Normal-arena `wait_match` deliberately returns `WaitMore` with `MatchedNpcInfo`: that is the extracted client's offline-battle branch. Live matches require the separate battle service.

## Local configuration

Edit `tables/ArenaGuildRules.json` before starting the server. These values are emulator rules where the original server behavior was unavailable; they do not reproduce original live balance.

| Setting | Default |
| --- | --- |
| Season epoch / duration | 2026-01-05 UTC / 7 days |
| Guild application phase | First 2 days; battles run for the rest of the season |
| Late guild-arena registration | Enabled, to allow small local servers to start playing mid-season |
| Minimum guild members | 1 |
| Normal arena / guild arena tickets | Refill to at least 5 / 3 each UTC day; retain larger balances |
| Normal arena win/loss | +20 / −10 score; 50 / 10 PvP Coins |
| Match expiry | 900 seconds |
| Guild donation | Up to 5 per account per contribution day; table ruby cost, local activity/points/materials |
| Guild attendance | 100 Guild Points and 100 guild activity; table reward milestones |
| Guild member reward cooldown | 24 hours, only where the building defines a reward |
| Legacy guild raids | Enabled despite the extracted campaign chapters being closed |
| Raid boss HP | 1,000,000,000 per boss |
| Maximum reported raid damage | 1,000,000,000 per elapsed second, capped at remaining HP |
| Arena reward currencies | Daily: PvP Coins; season: rubies; amounts/items from MatchReward |

Guild-arena pairing selects the first registered, unpaired guild with defense decks. It freezes each guild's roster when paired; later joiners cannot attack that session. There is one session per local season. A successful attack adds one session point, and season score is 1,000 plus session points. Table tiers determine season rewards, paid in Guild Arena Points and rubies with any listed items. Frozen roster members receive at most one season mail, even after changing guilds.

Guild raid tickets refill to at least the table-defined daily allowance. Contributions reset at the table's UTC reset hour; attendance and arena tickets reset at midnight UTC. Withdrawal restrictions use `GuildPenalty`; guild-raid and guild-arena entry/results enforce the content cooldown.

Changing the season epoch/duration on an existing database changes season identities. Keep those settings stable once players begin earning rewards.

## Data and migration

`ArenaGuildSupport.json` contains 29 table groups and 73 native service contracts. Re-export it using the same extracted client and decoded JIT data:

```powershell
python scripts/export_battle_data.py ..\kingsraid-table-data\output\TableJit ..\kingsraid-table-data\data\TableJit ..\kingsraid-table-data\dlls\Assembly-CSharp\Assembly-CSharp --profile arena-guild
```

The default exporter profile still produces `BattleSupport.json`. Guild support resolves unnamed MessagePack fields through the client's keyed schema and inherited properties.

Startup creates the community state/claim tables, guild applications, arena entries/scores, guild battle scores, defense decks and attack records. Existing guild/member records remain in place. No reset of the player database is needed.

## Remaining limits

- Live PvP/co-op sockets, friendly duels, ban/pick/World/Lucky arenas and cross-server federation are unavailable. Unsupported match types fail without charging. Global normal-arena queries describe the local server.
- Suppression references raid 90001, which is missing from the extracted RaidTable, and uses the unfinished party combat service. Its native session reports `NotHeld`. Registration fails, and there are no suppression combat rewards or populated competitive boards.
- Raid bonus/dummy encounters, chapter-final rewards, guild-arena session reward allocation, flags/server buffs and original matchmaking are unfinished. The extracted chapter reward rows do not cover the active local legacy chapters.
- Guild skill levels/votes/costs persist, but their gameplay effects still need integration into battle snapshots and reward calculations. Original Guild Point caps and bonuses are not applied. Some live guild notifications and achievement hooks remain unconnected.
- Original calendars are historical. Guild-arena captions may be blank for newer season indices even though the HTTP session schedule works.
- Results validate ownership, entry, party, expiry and duplicate submission; they do not simulate combat. Wins and bounded damage still come from the client.
- Native client playthroughs are still required. Automated checks cover server contracts and persistence, not Unity UI/combat behavior.

## Verification

```powershell
cargo test
cargo build --release
python scripts/smoke_community.py
python scripts/smoke_battle.py
```

Both HTTP scripts start hidden server processes with temporary databases and isolated ports. They never open the workspace `sprk.db`.
