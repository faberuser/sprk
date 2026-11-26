# Proposed calendar from the Pre-Doomsday archive

All ranges below are proposed local-server schedules, not recovered official
start/end dates. They repeat annually unless a different cadence is stated.
Ranges are inclusive calendar days in server time (currently UTC).

Source: `Z:/media/kingsraid/Pre-Doomsday/kingsraid-table-data/output/`.
Names were resolved against the archived English localization. Chapter IDs,
shop codes, portrait paths and reward definitions establish the content's
identity; they do not establish the proposed annual dates. Shop names beginning
with a year identify retained editions. Rotate those editions between years or
present them as separate story chapters during the same festival.

## Seasonal events

| Event/family                                                  | Proposed range                                   | Archived evidence                                                                                                       | Reason for placement                                                                                                             |
| ------------------------------------------------------------- | ------------------------------------------------ | ----------------------------------------------------------------------------------------------------------------------- | -------------------------------------------------------------------------------------------------------------------------------- |
| Lunar New Year shop                                           | Lunar New Year minus 7 days through plus 14 days | `LunarNewYearShop`, shop 18                                                                                             | Holiday-relative; the Gregorian dates must be selected for each year. No separate dungeon was identified for this shop.          |
| Spring Festival / Valentine's edition                         | February 1–28                                    | chapter 30034; `2020_Valentine_Shop`; `EVENT_20VALENTINE` portrait paths                                                | Seasonal identity is supported by the Valentine assets despite the generic English chapter title.                                |
| Exciting New Semester                                         | March 1–21                                       | chapter 30022; `2019_Semester_Shop`                                                                                     | Assumed spring school-term window.                                                                                               |
| Orvel Foundation Day                                          | March 15–31                                      | chapter 30042; `2021_Orvel_Shop`                                                                                        | An arbitrary annual celebration slot; March is a proposal, with no recovered foundation date.                                    |
| Outing Event, first edition                                   | April 1–21                                       | chapter 30023; `2019_Picnic_Shop`                                                                                       | Spring picnic theme.                                                                                                             |
| Children's Rainbow                                            | May 1–14                                         | chapter 30004; `ChildrensRainbowShop`                                                                                   | Children's festival theme. The chapter's English title key is unresolved, and it has no rows in the archived EventDungeonTable.  |
| Juno's Cooking Quest                                          | June 1–21                                        | chapter 30027; `2019_CookingLord_Shop`, `2021_CookingLord_Shop`                                                         | Flexible festival slot chosen to spread events through the year.                                                                 |
| Summer Festival / Orvel Beach / Orvel Water Park              | July 1–August 31                                 | chapters 30003, 30025, 30038; `SummerShop`, `2020_Summer_Shop`, `2021_Summer_Shop`                                      | Strong summer theme. Feature one edition at a time for 2–3 weeks.                                                                |
| Outing Event, second edition                                  | September 1–21                                   | chapter 30026; `2019_Picnic2_Shop`                                                                                      | Assumed autumn picnic slot; no original season/date was recovered.                                                               |
| Moon Rabbit LuLu event boss                                   | September 15–October 5                           | EventWorldBoss index 8002                                                                                               | Tentative autumn/moon-festival placement based on the boss's name. Not proof of an original Chuseok schedule.                    |
| Fall Sports Day: Muscle Festival                              | October 1–21                                     | chapter 30045; `21AUTUMNSPORTSDAY_SHOP`                                                                                 | Autumn sports-day identity; the chapter key itself says Halloween, while localized title and portrait paths identify sports day. |
| Halloween / Haunted Mansion Adventure                         | October 15–November 7                            | chapters 30001, 30010, 30028, 30044; `HalloweenShop`, `2018_Halloween_Shop`, `2021_Halloween_Shop`                      | Halloween theme. Keep older dungeon editions available as separate stories or alternate the featured edition.                    |
| Thanksgiving / Thanksgiving Defense                           | November 15–30                                   | chapter 30047; `ThanksgivingShop`; portal event code `EVENT_21THANKSGIVING`                                             | Proposed November holiday window. The older shop's exact regional holiday association is uncertain.                              |
| Grow Baby World Tree                                          | December 1–20                                    | `2021_WorldTree_Shop`; EventStepTable tree growth stages and WorldTree scene names                                      | Arbitrary winter community-event slot before Christmas. The growth mechanic itself has no inherent month.                        |
| Christmas / Troublesome Wish Box / Unexpected Present Dungeon | December 15–January 7                            | chapters 30002, 30020, 30021, 30032, 30033, 30048, 30049; `ChristmasShop`, `2019_Christmas_Shop`, `2021_Christmas_Shop` | Christmas theme; crosses the year boundary. Story, special and present dungeons can share one festival window.                   |
| Christmas event boss: The Unstoppable Lonely King Protianus   | December 15–January 7                            | EventWorldBoss indices 8001 and 8003; Christmas name/tactic keys                                                        | Pair with Christmas. These are separate retained definitions; do not assume both should run simultaneously.                      |

There is no separately identified Gregorian New Year festival chapter in this
inventory. The Lunar New Year shop is explicit; a generic January 1 festival
would be a new local event proposal.

## Story and hero events

These have no inherent seasonal date. Keep their stories permanently accessible;
if a timed event is desired, use an optional 14-day featured/reward period after
unlocking the content, or a rotation every two weeks. Assigning them to a
particular historical launch month would imply evidence that was not recovered.

| Event                                            | Chapter | Proposed availability                                                                                                   |
| ------------------------------------------------ | ------- | ----------------------------------------------------------------------------------------------------------------------- |
| Story of Ancient God King                        | 30024   | Permanent after its story prerequisites; optional 14-day feature                                                        |
| Where the Light of Fate Shines                   | 30030   | Permanent after prerequisites; retained `2019_DestinyLight_Shop` / `2021_DestinyLight_Shop` can rotate with the feature |
| Episode: Pansirone                               | 30031   | Permanent; optional 14-day feature                                                                                      |
| Episode: Xerah                                   | 30035   | Permanent; optional 14-day feature                                                                                      |
| Episode: Cycle of Fate (Fallen Kasel/Frey theme) | 30036   | Permanent; optional 14-day feature                                                                                      |
| Episode: Hilda                                   | 30037   | Permanent; optional 14-day feature                                                                                      |
| Episode: Rebel Clause                            | 30039   | Permanent; optional 14-day feature                                                                                      |
| Episode: Riheet & Ripine                         | 30040   | Permanent; optional 14-day feature                                                                                      |
| Episode: Shakmeh                                 | 30041   | Permanent; optional 14-day feature                                                                                      |
| Hero Special Dungeon (Isaiah title key)          | 30043   | Permanent; optional 14-day feature                                                                                      |
| Episode: Valance                                 | 30046   | Permanent; optional 14-day feature; portal code `VALANCE_SHOP`                                                          |
| Succeeding Will                                  | 30050   | Permanent story access with its raid/story prerequisites                                                                |
| Converging Destiny                               | 30051   | Permanent story access with its raid/story prerequisites                                                                |

## Other schedulable event systems

| Event/system                                          | Proposed cadence                                                     | Evidence and limits                                                                                                                                                                                               |
| ----------------------------------------------------- | -------------------------------------------------------------------- | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| Ophelia's Lucky Workshop, crafting and roulette       | June 1–21, September 22–October 12, December 15–January 7            | Portal code `EventCraft`; four roulette definitions; 74 crafting recipes in groups `210608_1`, `210928_1`, `210928_2`, `211221_1`, `211221_2`. Numeric prefixes are edition clues, not complete schedule records. |
| Enhancement Lab Challenge / Lakrak's Forge            | First 14 days of February, May, August and November                  | `Lakrak_Forge_Shop`; 15 EventForge levels. Quarterly dates are entirely a local rotation choice.                                                                                                                  |
| May's Special Shop / May's General Shop promotion     | First 7 days of each month                                           | `MaiGemShop`. May is the character's name; it does not imply the month of May.                                                                                                                                    |
| 1,000 Days Anniversary Festival                       | Server's 1,000th service day minus 7 days through plus 7 days        | chapter 30029; `1000Days_Shop`. A service milestone rather than an annual holiday. An optional annual rerun could share Foundation Day, with a clearly marked rerun title.                                        |
| Hero birthdays and anniversaries                      | Exact saved month/day; optional 7-day claim period starting that day | Separate MonthlyHero calendar; 99 archived birthday entries. The dates are actual table data, while the proposed claim duration is an assumption.                                                                 |
| King's Pass                                           | Calendar month, day 1 through last day                               | Archived mission definitions cover 11 season indices. A monthly recurrence and season rotation would be new server policy.                                                                                        |
| Attendance/login campaigns                            | 28-day campaign starting on the first of each month                  | Archived attendance/quiz/reward definitions exist, but they do not establish a complete identifiable seasonal campaign list. Keep native reward lengths when implementing a particular definition.                |
| Hot Time, EXP/gold/drop bonuses and upgrade discounts | Weekends; optional last weekend of each month for discounts          | Native EventBonusType supports these categories. This is a proposed generic bonus cadence, not a list of recovered live promotions.                                                                               |

## Inventory coverage and unresolved entries

The tables above account for all 37 named chapters in the archived 30000–39999
event range. The remaining entry is chapter 31001, `EVENTDUNGEON_NORMAL`, an
unresolved generic definition with no EventDungeonTable rows. It has no proposed
activation date until its identity is established.

EventDungeonTable also references chapters 40001–40006 (gold box, EXP potion,
magic powder, rune and fragment resource dungeons) and 50000 (The Shadow of
Aegina). These are separately classified resource/story activities, with no
holiday evidence; keep their existing unlock/recovery rules rather than assigning
annual festival dates. Challenge Tower's shop is marked EventOnly in ShopTable,
but that flag alone does not make the tower an annual holiday event. Likewise,
OrdealArenaEventTable describes arena modifiers, and EventTriggerTable includes
ordinary story scripts; their table names do not identify extra festivals.

This inventory covers named content and event systems recoverable from this
archive. Retired web-only promotions or missing server campaigns cannot be
enumerated from it. Enabling these events still requires server rules, reward
handling and validation of the retained assets. This document changes no running
event configuration.

Sources checked: CampaignChapterTable, EventDungeonTable, EventDungeonGroupTable,
PortalTable, PortalRenewalTable, ShopTable, EventCraftTable, EventRouletteTable,
EventForgeTable, EventStepTable, EventWorldBossTable, EventWorldBossSeasonTable,
MonthlyHeroTable, KingsPassSeasonMissionTable and attendance definitions.

## Implementation status

The seasonal events and the other schedulable systems above run on the server
from `Festivals` in `server/tables/LiveRules.json`
(`server/src/api/festivals.rs`). Story and hero events, the 1,000 Days festival,
hero birthdays and King's Pass are unchanged.

How a window opens content: at login the server sends the native
DropBonusEvents for open and upcoming runs (`LookaheadDays`). `EventNode`
reveals the festival's hidden chapter-99 world-map node, `EventDungeon` and
`EventShop` enable its NPC menus and Portal entries, and `EventCraft`,
`EventRoulette` and `GrowWorldTree` carry the crafting groups, roulettes and
World Tree. The server checks the same windows for battle entry, EventOnly
shops, crafting, roulettes, the forge, World Tree contributions and event
bosses. Clients see a change in schedule at their next login.

Within each run:

- All editions reachable in this client open together. Dungeon clears and
  story progress are permanent.
- Purchase limits of festival shops, Ophelia's craft counts and the World Tree
  community total and steps start over in each run.
- `ExpireCurrencies`/`ExpireItems` are cleared once the last overlapping
  festival using them closes, for example `EventDungeonPoint` after 7 January
  (Thanksgiving, World Tree and Christmas form one run). An account keeps a
  balance it had before it first saw the schedule until that balance's next
  run ends. Expired: `EventDungeonPoint`, `EventDungeonPoint2`,
  `GrowWorldTreePoint` and Ophelia's `EVENTCRAFT_MATERIAL_1`–`6`.
- Older chapters that define points per dungeon (30002, 30003, 30010, 30022,
  30023, 30026) now award them; later chapters drop points from their rewards.
- Weekend Hot Time: +50% EXP, +50% gold and +20% drops, Saturday and Sunday
  UTC, for campaign, custom campaign, party, boss, event dungeon, side quest and
  purchased dungeons. There is no upgrade discount.
- The event bosses are the World Bosses in their event forms
  (`client/managed/SprkWorldBossSkin`): neither this client nor the archive
  has a standalone event-boss screen. While `christmas-boss` is open,
  Protianus is The Unstoppable Lonely King (Christmas costume and field, its
  event art and name); while `moon-rabbit` is open, Mountain Fortress is Moon
  Rabbit LuLu. Stats, scoring, ranking and rewards stay the World Boss's, and
  the form shows only in the weeks the rotation brings that boss. The server
  signals the forms with the `EventShop` conditions `SprkWorldBossChristmas` and
  `SprkWorldBossMoonRabbit`; no standalone event boss (EventWorldBossTable) is
  scheduled, and battles of that type are refused.
- Thanksgiving Defense (chapter 30047, the only `PhaseStepRewardTable`
  dungeon) counts the K.O.s each battle reports. A battle's K.O.s reach a Daily
  level (0, 50, 100, 150, 200); each level pays once per day with that battle's
  loot. The run total unlocks the five milestones claimed in the K.O. rewards
  window (`phase_step/*`). One battle counts at most `PhaseStepMaxKills` (500).
- Lunar New Year (7 days before to 14 days after each date in `Around.Dates`,
  listed to 2036) and Children's Rainbow (1–14 May) are participation events:
  each day of the run an account logs in it is mailed the archived gift box
  (`PACKAGE_EVENT_NEWYEAR_BOX1`, `PACKAGE_CHILDRENSRAINBOW_BOX`), and after 7
  Lunar New Year days the New Year's Gratitude Gift (`PACKAGE_EVENT_NEWYEAR_BOX2`).
  Their shops stay closed: the archive has no items for them.

GM testing: `python server/gm.py world-boss protianus|mountain|xanadus|auto
[--hours N]` shows a chosen World Boss instead of the weekly rotation (to see
an event form, also open its festival); `python server/gm.py festivals` lists every festival and its current
or next run; `python server/gm.py festival <key> open|closed|auto --hours N`
overrides one server-wide until the time passes.

Not scheduled, because this client cannot reach the content or the archive has
none:

| Proposal row | Reason |
| --- | --- |
| Lunar New Year shop and New Year's Fortune points | No ShopItemTable rows to spend them on (the gifts above run instead) |
| Children's Rainbow shop and chapter 30004 | Shop has no items; the chapter has no dungeons (the gift above runs instead) |
| `HalloweenShop`, `ChristmasShop`, `ThanksgivingShop`, `SummerShop` | No ShopItemTable rows |
| Chapter 30001, `HalloweenShop` NPC | `NormalShop_Holloween` is not placed on any node |
| Chapter 30020 | `Square_Cristmas_2018_1` is not placed on any node |
| Chapters 30028, 30032, 30033 and `2019_*` shops replaced by `2021_*` editions | Their reruns (30044, 30048, 30049) replaced them on the world map |
| Attendance campaigns | The archive keeps their titles but not their rewards; the existing 28-day attendance is unchanged |

Client support (`client/tables/festivals/`):

- Portal shortcuts (Orvel): one per festival node, titled with the native
  event names, each shown only while its festival is open.
- The client places the Square's nodes at run time
  (`client/managed/SprkSquareLayout`): the side-quest node (41) and every
  festival node open now (revealed by `EventNode` "99,<node>") are spaced
  evenly on a ring around the central fountain, the side-quest node on the
  left and the festival nodes clockwise in node order, however many are open.
- Orvel's "EVENT" bubbles (WorldMapPropTable `UI.EventPopup*`) stay hidden
  (`SprkOrvelVisuals`): with several festivals open they crowd the town and
  cover the nodes. Orvel switches to the festival's own town map
  while it is open (`SprkSquareLayout.Map`): Christmas `Town_Christmas` (the
  snowy town with the snow globe; it wins over the World Tree from Dec 15),
  World Tree `Town2_WorldTree`, Halloween `Town_Halloween` and Orvel Foundation
  Day `Town_OrvelDays`, Lunar New Year `Town2_NewYear2022` ("2022" on its
  banners; the event announces a marker shop, `SprkLunarNewYear`, so the client
  can tell it is open). `Town_1000Days` and `Town2_NewYear2020/2021` are unused; `Town` and `Town_NewYear` are the
  old, smaller Orvel layout.
- Event dungeon tickets (`EventDungeonTicket`, 2 a day) are kept per event:
  every event chapter has its own balance and recharge. The server uses the
  chapter of the event battle being started. After login the client loads
  every event's balance (`/sprk/event_ticket` without a chapter) and keeps
  each one current from later ticket results, so an event's menu or dungeon
  list shows its own count at once; opening it also tells the server which
  event is in use and refreshes the count (`SprkEventTicket`).
- Lakrak's Forge: while it runs, each daily mission claimed also pays
  `DailyMissionCoins` (5) Orvel Castle Gold Coins, as the archived event text
  describes ("earned through Daily Missions"; the archive has no amounts). A
  failed enhancement pays a third of that level's Guaranteed Enhancement
  remnants (rounded up), a break half (at least 1). The coins, ores and
  remnants expire with the run.
- Event dungeon stages of Christmas 2017 (30002) and Summer (30003) carried
  the fixed open dates of their 2018/2019 runs (`OpenCheckTime`), so every
  stage showed Closed; the table patch drops those dates and keeps the
  clear-the-previous-stage rule (`40-event-dungeon-open-times.json`).
- The 2017 Christmas dungeon's menu (`Square2`, missing from the archive too)
  uses the 2018 Christmas square's background, music and NPC.
