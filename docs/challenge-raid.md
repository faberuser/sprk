# Challenge Raid

Challenge Raid uses the native lobby, affix selector, solo party screen and
cooperative battle worker. Login supplies `ChallengeRaidNotice`,
`ChallengeRaidSeason` and `ChallengeRaid` events for the current and following
week. The server validates the active raid, stage, hero level, party size,
affix group, advantage/penalty balance and affix hero bans before entry.

`BattleRules.json` defines the SPRK weekly rotation: the eight archived 700xx
bosses, alternating solo and cooperative weeks. Dates follow `SeasonEpoch` and
`SeasonDays`; this schedule is local policy, not a recovered production calendar.
The original Raid, CampaignDungeon and ChallengeRaid tables already contain
these stages. No replacement combat stats or boss assets are needed.

`ChallengeRaidSupport.json` preserves the decoded Affix table from
`Z:/media/kingsraid/Pre-Doomsday/kingsraid-table-data/output/MessagePack/AffixTable.json`
with its source SHA-256. The client `AffixHelper.GetTotalAffixPoint` specifies
`(ClearPoint + sum(Affix.Point)) * AffixMultiple / 1000`.
Successful clears use that formula from the saved entry, never submitted damage.
Scores retain the best clear; shorter combat time wins ties. Native ranks are
zero based, an unranked player receives -1, and solo pages contain ten entries.
Failed attempts do not replace the best record. A battle crossing a season
boundary cannot earn a new-season score or rewards.

Cooperative parties run through the authenticated native worker. Its builder
receives the room's validated affixes before initializing combat. Team rankings
retain the best attempt for the exact participant roster. Solo ranking prizes
come from ChallengeRaidReward and are delivered once through the mailbox after
season close, including on reconnect. The archive has no 700xx cooperative
ranking-prize or clear-achievement rows; this implementation does not invent
those rewards. Earlier 500xx/600xx variants are outside this rotation.

Tests cover entry rejection, preserved best scores and times, pagination,
season rollover, repeat settlement, cooperative affix propagation and roster
records. `native_challenge_party_combat_and_settlement` is an opt-in Unity
integration test using `CONQUEST_NATIVE_EXECUTABLE`. Client build verification
runs through `--build-release` and `--verify-arcdim-worker`.

Restart the game server and native battle worker after installing this change;
players should relog to receive the new events. Local signed update generation
does not deploy the server or publish a GitHub release.
