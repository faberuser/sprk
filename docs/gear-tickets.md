# Gear tickets

`item/use_equip_option_select_item` accepts Unity WWWForm arrays (repeated
`EquipOptionIndices` and `EquipExtraOptionIndices` fields) and JSON arrays.
Order and duplicate values are preserved. An omitted normal-option array means
no chosen normal stats, as sent by tickets that only select a Technomagic skill.
Chosen normal options must match the ticket's selection count, capped by the
equipment's available slots. Invalid requests roll back the whole transaction.

Repeated normal stats are intentional where the equipment's
`EnableDuplicationOption` is true, including Technomagic and T8 Legendary gear.
Other gear retains its duplicate restriction. Additional Technomagic skills use
their own option pools and duplication rules. A ticket's number of selectable
normal stats and whether it selects an additional skill remain table driven.

The Pre-Doomsday archive comparison checks all 498 selector definitions and
2,686 equipment records, including normal/unique option pools, selection counts,
duplication flags, rune counts, star and enhancement levels:

```powershell
python scripts/audit_gear_ticket_rules.py --archive 'Z:\media\kingsraid\Pre-Doomsday' --report target/gear-ticket-rules/archive-audit.json
cargo test gear_tickets_ -- --nocapture
```

The endpoint tests redeem a representative equipment choice from all 459 gear
ticket definitions using native and JSON forms, check returned stats, and test invalid
selections for rollback. Dedicated T8/Technomagic cases also verify that selected
duplicate stats survive native decoding and database persistence.
The remaining 39 archive selectors grant Soulstones or other non-gear rewards
and use separate endpoints.

This compatibility fix changes only the server; restart the game server after
installing the rebuilt executable. No client asset changes are required.
