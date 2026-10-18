# League of Honor weekly and season rewards

`tables/HonorRewards.json` restores all 43 archived rank brackets, 52 referenced
reward bundles and the top-three season booster items. The importer verifies the
older 14-field tier layout against the archived build's IL2CPP metadata. This
avoids confusing the newer client's 20-field layout with the pre-Doomsday data.
Source hashes are included in the generated file.

```powershell
python scripts/import_honor_rewards.py --archive 'Z:/media/kingsraid/Pre-Doomsday'
```

SPRK closes weekly rewards on Monday at 00:00 UTC. The archive confirms Monday
(`MatchingBanPickRewardWeekDay=1`); its original server timezone and cutoff hour
are not available. Completing an Honor duel records a weekly score snapshot. The
final snapshots rank eligible weekly participants by score, wins, then account ID;
idle accounts and League of Victory scores do not count. A completed forfeit is a
loss; a canceled/refunded match creates no weekly entry. This participation and
tie policy is SPRK's local policy, since the original server implementation is
not available. Historical reward amounts and item bundles come from the archive.

Weekly mail grants the tier's `WeeklyReward` as **RankingPoint** plus its
`WeeklyBoxReward` bundle. Bronze receives 3 Ranking Points and seven Bronze
chests; rank 1 receives 150 Ranking Points and seven Champion chests. Chests use
the existing package/drop tables, including currency outcomes such as gold.

Season mail grants `SeasonReward` as **RankingPoint**, the items referenced by
`ShowRewardIndex`, and the archived gold booster item for ranks 1–3. Bronze gets
120 Ranking Points, 10 Soulstone Fragment selectors, 50 Unique Weapon fragments
and 50 Unique Treasure fragments. Rank 1 gets 15,000 Ranking Points, the Champion
costume selector, three customization selectors, 1,500 Soulstone Fragment
selectors, 2,000 fragments of each Unique type, and the first-place booster.

Season payouts use closed Honor `arena_scores` records and SPRK's existing
`SeasonEpoch`/`SeasonDays` calendar. The default local season is seven days;
the expired historical 2017–2024 season dates are not replayed in 2026. Weekly
and season rewards are independent entitlements even when their boundaries
coincide. Current/open periods cannot pay early.

Login, Arena requests and native mailbox polling settle due rewards. Each mail
and its `community_claims` entitlement commit in the same transaction. Repeated
requests and restarts cannot duplicate rewards. Mail attachments are granted
through the existing atomic claim flow. `HonorRewardMailDays` controls mailbox
expiry after delivery (default seven days). The mailbox notification service
announces the new mail normally.

Weekly tracking begins with duels completed after this change; earlier weeks
cannot be reconstructed reliably from the old records. Existing closed Honor
season scores can receive their previously unpaid season reward. Historical
server-wide `ServerBuff` effects are not part of the individual payout.

Per-duel GloryPoint rewards remain the separate local configuration documented
in [honor-medal-payout.md](honor-medal-payout.md).

Validation covers every rank bracket and bundle, deterministic ties, exclusion
of idle/Victory scores, open-period gating, retry after missing data, duplicate
settlement, actual mail claims, chest opening and persistent RankingPoint balances.
