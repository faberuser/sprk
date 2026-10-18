# League of Honor duel rewards

Completed League of Honor (`BanPick`) duels award `GloryPoint`, the native Honor
Medal currency used by the Honor shop. League of Victory continues to award
`PvpCoin`.

`tables/ArenaGuildRules.json` controls the local payout:

- `HonorAttendanceMedals`: 10 for completing a duel, including a recorded loss
  or forfeit.
- `HonorWinBonusMedals`: 40 additional medals for winning (50 total).

These are emulator defaults, not a verified reconstruction of the archived
tier-specific reward amounts. Queue cancellation and interrupted matches that
are refunded do not award medals. Settlement, balance updates and saved receipts
share the existing transaction; duplicate worker reports and result requests
reuse the receipt without paying again.

The client Honor result initializer opens the same native currency/item reward
popup used by Victory, after the result animation. Login also restores
`UserInfo.GloryPoint`, which the native Honor UI uses for its balance display.
Previously completed matches are not retroactively converted or paid again.
Weekly/season rewards are separate from this per-duel payout; see
[the historical reward implementation](honor-historical-rewards.md).

Validation covers wins, losses, forfeits, cancellation, duplicate settlement,
receipt replay, persistent login balances and preservation of Victory currency.
