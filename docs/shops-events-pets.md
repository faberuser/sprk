# Shops, summons, events, and pets

The server implements the client's non-cash HTTP flows in `src/api/live/`. Real-money checkout, receipts, platform verification, and payment callbacks remain unavailable.

## Data and configuration

- `tables/LiveSupport.json` contains exported client contracts and tables: summon/ticket definitions, selection shops, event recipes/roulette/forging/steps, purchase dungeons, and pets.
- `tables/LiveRules.json` supplies the missing local catalog, schedules, reward-pool assignments, pity rules, and pet rules. These defaults are **local approximations**, not the original live service's economy.
- Changes take effect after restarting the server. `Enabled`, `Begin`, and `End` control offers, banners, and events; times are UTC Unix seconds, with zero meaning no boundary.
- Existing accounts use the existing `extension_state`, item, equipment, currency, and booster tables. No database reset is required.

To regenerate the recovered data, from the server directory:

```powershell
python scripts/export_live_data.py ..\kingsraid-table-data\output\TableJit ..\kingsraid-table-data\data\TableJit ..\kingsraid-table-data\dlls\Assembly-CSharp\Assembly-CSharp
```

## Non-cash shops

Regular shops support all extracted `ShopCostType` currencies. New event balances share `battle_currencies` and login's `PlayerCurrencyInfos`; existing gold, rubies, friendship, mileage, and other established balances retain their storage.

`Products` is an explicit non-cash catalog. Set `PriceType` to a client currency enum value, `Price`, `ItemInfos`, currency rewards, and `PurchasableCount`. `ResetSeconds` identifies fixed UTC purchase periods; zero means a lifetime limit. The supplied catalog includes ruby stamina/egg bundles, gold event supplies, and selection-shop offers. Products omitted from this catalog cannot be purchased through the new handler.

`SelectionCategory` connects a product to an extracted selection group. The server rolls and saves its displayed contents until restock; purchases grant those exact contents. Restock uses `SelectionRestockGem` and `SelectionMaxResets`. Restocking does not reset purchase limits.

`DiscountPercent` and `DiscountLimit` implement a discount for the first configured number of purchases across accounts in a purchase period. Costs are recalculated in the transaction, including when buying through a direct request. `RightAway` exposes an active offer through the client's timed-offer listing.

Purchase dungeons use extracted prices and booster durations. The purchase grants timed access; battle entry requires that unexpired booster. These dungeons use the existing campaign entry/result/reward pipeline.

## Equipment and pet summons

`Summons` maps client gacha indices to local availability and reward groups. The supplied definitions enable normal, special, pickup, artifact, all-in-one, one step-up banner, and regular pet summons. Other historical banners stay unavailable until configured.

- Free draws enforce the table's daily allowance and cooldown.
- Tickets must belong to the requested banner and category. Ruby/gold draws charge the table price; submitted discount amounts are never trusted.
- Category pools use extracted class groups. Pickup bonus draws use the selected heroes' extracted groups.
- Step-up progression follows `StepUpGachaReward`, including discounts and milestone rewards.
- `PityEvery`/`PityGroup` grant a guaranteed pool at each configured draw interval. `CeilingCount`/`CeilingItem` define a separately claimed reward; claims consume accumulated count. Optional `StarterCount` counts completed summon requests; `StarterRewardIndex` (or `StarterItem`) defines its once-only final reward. The supplied starter banner uses the extracted final reward.
- Counts, free use, step-up progress, and claims survive reconnects and restarts. Failed grants, including full equipment storage, roll back costs and progress.

The equipment endpoint supports pet results as used by the current client. The legacy pet summon endpoint is also available. Pet duplicates become the appropriate soul item.

## Events

`Events` controls a local calendar, recipe groups, roulette indices, forging, and World Tree contributions. `Season` identifies step progression; changing it starts new progress and claims. Daily steps reset at UTC midnight.

- Crafting consumes every recipe material, checks craft limits, and grants the table reward atomically.
- Roulette charges its table currency/item cost and rolls its extracted reward group.
- Forging validates the owned equipment and next level, charges materials/gold, and applies success/failure/destruction probabilities. Guaranteed forging also consumes the table's extra material. Exchange consumes the equipment and grants its level's reward. Locked, equipped, preset, and pending equipment cannot be consumed.
- World Tree contributions spend `GrowWorldTreePoint`. `Contribution` and `DailyContributionLimit` are local settings. Personal, daily, and shared-server thresholds use `EventStep`; each account claims each reached step once. Shared rewards require participation.

## Pets

Pet ownership is shared with existing selectors and generic item rewards. The server supports duplicate souls, legend awakening, soul tier upgrades, food, interactions, happiness gifts, companion/avatar selection, house placement, incubator purchases and slots, eggs, and exploration.

Egg duration and incubator speed come from extracted tables. Eggs are consumed when set, returned on cancellation, and rewarded once after the stored completion time. Pet food uses the existing `PetFeedKey` balance and its configured daily refill.

Exploration consumes food per pet, reserves participants, and stores its reward at departure. Completion checks the timer and grants the reward once; cancellation applies the extracted penalty. `PetExploreDecks`, `PetExploreRewardPerPet`, `EggSupplierSeconds`, `EggSupplierItem`, and `PetTierGroups` supply rules absent from the original server data. `PetSoulItems` explicitly allows tier-upgrade materials.

## Validation and remaining limits

Validated: all 144 Rust tests pass (including 12 new feature tests), the release build succeeds, and `scripts/smoke_live.py` passes HTTP flows and recovery after restarting its temporary server.

Run `cargo test`, then `cargo build --release` and `python scripts/smoke_live.py`. The HTTP smoke test uses a temporary database and restarts its own server to verify recovery; it does not touch the development database.

Native client playthroughs are still required. Original campaign marketing triggers, remote promotional artwork/web pages, original server prices/pools/schedules, and pet combat-bonus integration are not reconstructed. The local catalog and historical events may need matching client visibility settings. Seasonal subscription products and cash-funded entitlements are not generated by these offers. Combat verification retains the limitations of the existing campaign implementation.
