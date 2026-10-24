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

| Event/family | Proposed range | Archived evidence | Reason for placement |
| --- | --- | --- | --- |
| Lunar New Year shop | Lunar New Year minus 7 days through plus 14 days | `LunarNewYearShop`, shop 18 | Holiday-relative; the Gregorian dates must be selected for each year. No separate dungeon was identified for this shop. |
| Spring Festival / Valentine's edition | February 1–28 | chapter 30034; `2020_Valentine_Shop`; `EVENT_20VALENTINE` portrait paths | Seasonal identity is supported by the Valentine assets despite the generic English chapter title. |
| Exciting New Semester | March 1–21 | chapter 30022; `2019_Semester_Shop` | Assumed spring school-term window. |
| Orvel Foundation Day | March 15–31 | chapter 30042; `2021_Orvel_Shop` | An arbitrary annual celebration slot; March is a proposal, with no recovered foundation date. |
| Outing Event, first edition | April 1–21 | chapter 30023; `2019_Picnic_Shop` | Spring picnic theme. |
| Children's Rainbow | May 1–14 | chapter 30004; `ChildrensRainbowShop` | Children's festival theme. The chapter's English title key is unresolved, and it has no rows in the archived EventDungeonTable. |
| Juno's Cooking Quest | June 1–21 | chapter 30027; `2019_CookingLord_Shop`, `2021_CookingLord_Shop` | Flexible festival slot chosen to spread events through the year. |
| Summer Festival / Orvel Beach / Orvel Water Park | July 1–August 31 | chapters 30003, 30025, 30038; `SummerShop`, `2020_Summer_Shop`, `2021_Summer_Shop` | Strong summer theme. Feature one edition at a time for 2–3 weeks. |
| Outing Event, second edition | September 1–21 | chapter 30026; `2019_Picnic2_Shop` | Assumed autumn picnic slot; no original season/date was recovered. |
| Moon Rabbit LuLu event boss | September 15–October 5 | EventWorldBoss index 8002 | Tentative autumn/moon-festival placement based on the boss's name. Not proof of an original Chuseok schedule. |
| Fall Sports Day: Muscle Festival | October 1–21 | chapter 30045; `21AUTUMNSPORTSDAY_SHOP` | Autumn sports-day identity; the chapter key itself says Halloween, while localized title and portrait paths identify sports day. |
| Halloween / Haunted Mansion Adventure | October 15–November 7 | chapters 30001, 30010, 30028, 30044; `HalloweenShop`, `2018_Halloween_Shop`, `2021_Halloween_Shop` | Halloween theme. Keep older dungeon editions available as separate stories or alternate the featured edition. |
| Thanksgiving / Thanksgiving Defense | November 15–30 | chapter 30047; `ThanksgivingShop`; portal event code `EVENT_21THANKSGIVING` | Proposed November holiday window. The older shop's exact regional holiday association is uncertain. |
| Grow Baby World Tree | December 1–20 | `2021_WorldTree_Shop`; EventStepTable tree growth stages and WorldTree scene names | Arbitrary winter community-event slot before Christmas. The growth mechanic itself has no inherent month. |
| Christmas / Troublesome Wish Box / Unexpected Present Dungeon | December 15–January 7 | chapters 30002, 30020, 30021, 30032, 30033, 30048, 30049; `ChristmasShop`, `2019_Christmas_Shop`, `2021_Christmas_Shop` | Christmas theme; crosses the year boundary. Story, special and present dungeons can share one festival window. |
| Christmas event boss: The Unstoppable Lonely King Protianus | December 15–January 7 | EventWorldBoss indices 8001 and 8003; Christmas name/tactic keys | Pair with Christmas. These are separate retained definitions; do not assume both should run simultaneously. |

There is no separately identified Gregorian New Year festival chapter in this
inventory. The Lunar New Year shop is explicit; a generic January 1 festival
would be a new local event proposal.

## Story and hero events

These have no inherent seasonal date. Keep their stories permanently accessible;
if a timed event is desired, use an optional 14-day featured/reward period after
unlocking the content, or a rotation every two weeks. Assigning them to a
particular historical launch month would imply evidence that was not recovered.

| Event | Chapter | Proposed availability |
| --- | --- | --- |
| Story of Ancient God King | 30024 | Permanent after its story prerequisites; optional 14-day feature |
| Where the Light of Fate Shines | 30030 | Permanent after prerequisites; retained `2019_DestinyLight_Shop` / `2021_DestinyLight_Shop` can rotate with the feature |
| Episode: Pansirone | 30031 | Permanent; optional 14-day feature |
| Episode: Xerah | 30035 | Permanent; optional 14-day feature |
| Episode: Cycle of Fate (Fallen Kasel/Frey theme) | 30036 | Permanent; optional 14-day feature |
| Episode: Hilda | 30037 | Permanent; optional 14-day feature |
| Episode: Rebel Clause | 30039 | Permanent; optional 14-day feature |
| Episode: Riheet & Ripine | 30040 | Permanent; optional 14-day feature |
| Episode: Shakmeh | 30041 | Permanent; optional 14-day feature |
| Hero Special Dungeon (Isaiah title key) | 30043 | Permanent; optional 14-day feature |
| Episode: Valance | 30046 | Permanent; optional 14-day feature; portal code `VALANCE_SHOP` |
| Succeeding Will | 30050 | Permanent story access with its raid/story prerequisites |
| Converging Destiny | 30051 | Permanent story access with its raid/story prerequisites |

## Other schedulable event systems

| Event/system | Proposed cadence | Evidence and limits |
| --- | --- | --- |
| Ophelia's Lucky Workshop, crafting and roulette | June 1–21, September 22–October 12, December 15–January 7 | Portal code `EventCraft`; four roulette definitions; 74 crafting recipes in groups `210608_1`, `210928_1`, `210928_2`, `211221_1`, `211221_2`. Numeric prefixes are edition clues, not complete schedule records. |
| Enhancement Lab Challenge / Lakrak's Forge | First 14 days of February, May, August and November | `Lakrak_Forge_Shop`; 15 EventForge levels. Quarterly dates are entirely a local rotation choice. |
| May's Special Shop / May's General Shop promotion | First 7 days of each month | `MaiGemShop`. May is the character's name; it does not imply the month of May. |
| 1,000 Days Anniversary Festival | Server's 1,000th service day minus 7 days through plus 7 days | chapter 30029; `1000Days_Shop`. A service milestone rather than an annual holiday. An optional annual rerun could share Foundation Day, with a clearly marked rerun title. |
| Hero birthdays and anniversaries | Exact saved month/day; optional 7-day claim period starting that day | Separate MonthlyHero calendar; 99 archived birthday entries. The dates are actual table data, while the proposed claim duration is an assumption. |
| King's Pass | Calendar month, day 1 through last day | Archived mission definitions cover 11 season indices. A monthly recurrence and season rotation would be new server policy. |
| Attendance/login campaigns | 28-day campaign starting on the first of each month | Archived attendance/quiz/reward definitions exist, but they do not establish a complete identifiable seasonal campaign list. Keep native reward lengths when implementing a particular definition. |
| Hot Time, EXP/gold/drop bonuses and upgrade discounts | Weekends; optional last weekend of each month for discounts | Native EventBonusType supports these categories. This is a proposed generic bonus cadence, not a list of recovered live promotions. |

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
