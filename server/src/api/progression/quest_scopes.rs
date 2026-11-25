//! Restore known server-only mission filters without treating missing data as
//! a wildcard. Goal keys are stable identifiers from the archived localization;
//! display variables are used only where the goal defines their exact meaning.
use super::*;
impl View {
    pub(super) fn scoped_quest(&self, s: &AppState, row: &Value) -> Option<i64> {
        let goal = row["Goal"].as_str().unwrap_or("");
        let kind = row["Kind"].as_str().unwrap_or("");
        let required = || row["Variable3"].as_array().and_then(|v| {
            let parts: Option<Vec<&str>> = v.iter().map(Value::as_str).collect();
            parts?.concat().parse::<i64>().ok().filter(|v| *v > 0)
        });
        let count = |metric: &str, chapters: &[i64], types: &[i64]| if chapters.is_empty() && !types.is_empty() {
            self.typed_metric(s, metric).iter().filter(|((battle, _), _)| types.contains(battle)).map(|(_, v)| *v).sum::<i64>()
        } else {
            self.metrics_of(metric).iter()
                .filter(|(_,k,c,d,_)| k == metric && (chapters.is_empty() || chapters.contains(c))
                    && (types.is_empty() || self.battle_type(s,*c,*d).is_some_and(|t|types.contains(&t))))
                .map(|v|v.4).sum::<i64>()
        };
        let progress = |value: i64| {
            if n(row,"ReqProgress") > 0 { value }
            else { required().is_some_and(|target| value >= target) as i64 }
        };
        let equipment_matches = |item: i64, class: &str| {
            let Some((part,detail,equip_type))=self.equipment_metadata(s,item) else {return false;};
            match class {
                // Refined Mana Stone: the treasure any Hero can wear (not a Unique Treasure).
                "mana_stone" => part==7 && detail!=90004,
                "artifact" => part==5 && detail==90002,
                "weapon" => part==1 && detail==90001,
                "class_weapon" => part==1 && detail==90003,
                "unique_treasure" => part==7 && detail==90004,
                "treasure" => part==7,
                "legendary" => matches!(detail,6001|6002),
                "technomagic" => equip_type==1,
                _ => false,
            }
        };
        let equipped = |class: &str| self.equipment().iter().any(|e|
            equipment_matches(n(e,"ItemIndex"),class) && n(e,"InventoryType")==0 &&
            self.equipped_slots().contains(&n(e,"SlotIndex"))) as i64;
        let action_count = |action: &str, class: &str| self.metrics_of(action).iter()
            .filter(|(_,k,item,_,_)|k==action && equipment_matches(*item,class)).map(|m|m.4).sum::<i64>();
        Some(match (kind, goal) {
            ("WearEquip", "MAIN_MISSION_GOAL_16" | "MILESTONE_MISSION_GOAL_97" | "SUPERPASS_MISSION_GOAL_FREY_10" | "SUPERPASS_MISSION_GOAL_KASEL_10") => equipped("artifact"),
            ("WearEquip", "MAIN_MISSION_GOAL_44") => equipped("treasure"),
            ("WearEquip", "MILESTONE_MISSION_GOAL_53") => equipped("class_weapon"),
            ("WearEquip", "MILESTONE_MISSION_GOAL_54") => equipped("weapon"),
            ("WearEquip", "MILESTONE_MISSION_GOAL_59") => equipped("unique_treasure"),
            ("WearEquip", "MILESTONE_MISSION_GOAL_48") => equipped("legendary"),
            ("AwakenGear", "MILESTONE_MISSION_GOAL_50" | "MILESTONE_MISSION_GOAL_57" | "MILESTONE_MISSION_GOAL_63") => {
                let class=match goal {"MILESTONE_MISSION_GOAL_50"=>"legendary","MILESTONE_MISSION_GOAL_57"=>"weapon",_=>"unique_treasure"};
                let target=required()?;
                (self.equipment().iter().any(|e|equipment_matches(n(e,"ItemIndex"),class) && n(e,"Star")>=target)
                    || self.metrics_of("AwakenGear").iter().any(|(_,k,item,star,v)|k=="AwakenGear" && *v>0 && *star>=target && equipment_matches(*item,class))) as i64
            }
            ("AwakenGear", "MAIN_MISSION_GOAL_25") => {
                let target=required()?;
                self.metrics_of("AwakenGear").iter().any(|(_,k,item,star,v)|k=="AwakenGear" && *item>0 && *star>=target && *v>0) as i64
            }
            ("AwakenGear", "MILESTONE_MISSION_GOAL_128") => action_count("AwakenGear","technomagic"),
            ("UpgradeEquipTier", "MILESTONE_MISSION_GOAL_129") => action_count(kind,"technomagic"),
            ("UpgradeEquipOption", "MAIN_MISSION_GOAL_57") | ("RenewEquipOption", "MAIN_MISSION_GOAL_56") |
            ("Enchant", "MAIN_MISSION_GOAL_60") => self.metric(kind,"all",None,None),
            ("UpgradeEquipOption", "MILESTONE_MISSION_GOAL_51") | ("RenewEquipOption", "MILESTONE_MISSION_GOAL_52") => action_count(kind,"legendary"),
            ("RenewEquipOption", "MILESTONE_MISSION_GOAL_60") => action_count(kind,"unique_treasure"),
            ("RenewEquipOption", "MILESTONE_MISSION_GOAL_127") | ("Enchant", "MISSION_GOAL_1000376") => action_count(kind,"technomagic"),
            ("ArtifactRestore", "MAIN_MISSION_GOAL_15" | "SUPERPASS_MISSION_GOAL_FREY_09" | "SUPERPASS_MISSION_GOAL_KASEL_09" | "MILESTONE_MISSION_GOAL_96") |
            ("EquipRune", "MAIN_MISSION_GOAL_18" | "SUPERPASS_MISSION_GOAL_CLAUSE_07" | "MILESTONE_MISSION_GOAL_55") |
            ("SoulRestore", "MAIN_MISSION_GOAL_48") | ("SoulWeaponRenewOption", "MILESTONE_MISSION_GOAL_66") => self.metric(kind,"all",None,None),
            ("UseItem", "MAIN_MISSION_GOAL_11") => self.metrics_of("UsePotionItem").iter().filter(|(_,k,item,_,_)|
                k=="UsePotionItem" && s.tables.inventory.potions.get(&(*item as i32)).is_some_and(|r|n(r,"ActionType")==3))
                .map(|m|m.4).sum(),
            ("GiveFriendlyPoint", "MILESTONE_MISSION_GOAL_105") => {
                let npc=row["Variable3"].as_array()?.first()?.as_str()?.strip_prefix("CREATURE_NAME_")?.parse::<i64>().ok()?;
                self.metric(kind,"all",Some(npc),None)
            }
            ("ClearDungeon", "MAIN_MISSION_GOAL_6") => progress(count("ClearDungeon", &[20001], &[37])),
            ("ClearDungeon", "MAIN_MISSION_GOAL_9_1") => progress(count("ClearDungeon", &[20002], &[37])),
            ("ClearDungeon", "MILESTONE_MISSION_GOAL_115") => progress(count("ClearDungeon", &[], &[37])),
            ("ClearDungeon", "MILESTONE_MISSION_GOAL_78" | "ESTELLE_SPECIAL_MISSION_GOAL_5002110") => {
                required().is_some_and(|target| self.guild_damage().get("raid").copied().unwrap_or(0) >= target) as i64
            }
            ("ClearDungeon", "MAIN_MISSION_GOAL_11582") => progress(count("ClearDungeon", &[], &[40])),
            ("ClearDungeon", "MAIN_MISSION_GOAL_11791") => progress(count("ClearDungeon", &[], &[41])),
            ("ClearDungeon", "MAIN_MISSION_GOAL_11792") => progress(count("ClearDungeon", &[], &[42])),
            ("ClearDungeon", "MAIN_MISSION_GOAL_14" | "MILESTONE_MISSION_GOAL_2" | "SUPERPASS_MISSION_GOAL_FREY_08" | "SUPERPASS_MISSION_GOAL_KASEL_08"
                | "MAIN_MISSION_GOAL_34" | "MILESTONE_MISSION_GOAL_1") => {
                let tower = if matches!(goal,"MAIN_MISSION_GOAL_34"|"MILESTONE_MISSION_GOAL_1") {11} else {21};
                let floor = self.metrics_of("ClearDungeon").iter().filter(|(_,k,_,_,v)| k=="ClearDungeon" && *v>0)
                    .filter_map(|(_,_,c,d,_)|self.tower_floor(s,tower,*c,*d))
                    .max().unwrap_or(0);
                required().is_some_and(|target| floor>=target) as i64
            }
            ("ClearDungeon", "MILESTONE_MISSION_GOAL_118") => progress(self.metric("CompleteWorldBoss","all",None,None)),
            ("EnterDungeon", "SUPERPASS_MISSION_GOAL_KASEL_12") => progress(count("EnterDungeon", &[], &[16])),
            ("EnterDungeon", "MAIN_MISSION_GOAL_55_1") => progress(count("EnterDungeon", &[], &[25,30])),
            ("ClearDungeon" | "EnterDungeon", "MAIN_MISSION_GOAL_47" | "SPECIAL_MISSION_GOAL_5000970") => progress(count(if kind=="ClearDungeon" {"ClearDungeon"} else {"EnterDungeon"}, &[], &[35])),
            ("BuyShopItem", "MILESTONE_MISSION_GOAL_79") => progress(self.metric("BuyShopItem","all",Some(20),None)),
            ("BuyShopItem", "ESTELLE_SPECIAL_MISSION_GOAL_5001990") => progress(self.metric("BuyShopItem","all",Some(21),None)),
            ("BuyShopItem", "MILESTONE_MISSION_GOAL_76" | "MAIN_MISSION_GOAL_36_1" | "MILESTONE_MISSION_GOAL_26" | "MILESTONE_MISSION_GOAL_113" | "MAIN_MISSION_GOAL_31") => {
                let shop=match goal {"MILESTONE_MISSION_GOAL_76"=>27,"MAIN_MISSION_GOAL_36_1"=>12,"MILESTONE_MISSION_GOAL_26"=>11,"MILESTONE_MISSION_GOAL_113"=>15,_=>1};
                self.metric("BuyShopItem","all",Some(shop),None)
            }
            ("AwakenGear", "SUPERPASS_MISSION_GOAL_AWAKENGEAR_01") => self.metric("AwakenGearAttempt","all",None,None)
                + self.metric("AwakenGear","all",Some(0),None),
            // Battle tab. The Royal Underground Labyrinth is the seven
            // TowerType.Maze towers (1001-1007), each with floors B1-B5.
            ("ClearDungeon", "MILESTONE_MISSION_GOAL_3") => {
                let target = required()?;
                let floor = self.metrics_of("ClearDungeon").iter()
                    .filter(|(_,k,_,_,v)| k == "ClearDungeon" && *v > 0)
                    .filter_map(|(_,_,c,d,_)| LABYRINTHS.filter_map(|t| self.tower_floor(s, t, *c, *d)).max())
                    .max().unwrap_or(0);
                (floor >= target) as i64
            }
            ("ClearDungeon", "MAIN_MISSION_GOAL_51") => progress(self.metrics_of("ClearDungeon").iter()
                .filter(|(_,k,c,d,_)| k == "ClearDungeon" && LABYRINTHS.into_iter().any(|t| self.tower_floor(s, t, *c, *d).is_some()))
                .map(|m| m.4).sum()),
            // "Deal {N} DMG to <World Boss>": best single battle against any
            // season's version of that boss (WorldBoss rows sharing its name).
            ("ClearDungeon", "MILESTONE_MISSION_GOAL_4" | "MILESTONE_MISSION_GOAL_5" | "MILESTONE_MISSION_GOAL_6") => {
                let bosses: &[i64] = match goal {
                    "MILESTONE_MISSION_GOAL_4" => &[1, 3, 97],  // Mountain Fortress
                    "MILESTONE_MISSION_GOAL_5" => &[2, 4, 98],  // Protianus
                    _ => &[5, 99],                              // Xanadus
                };
                let target = required()?;
                let best = self.hist(super::state::part::WORLD_RANK).world_boss_best.iter()
                    .filter(|(boss, _)| bosses.contains(boss)).map(|(_, v)| *v).max().unwrap_or(0);
                (best >= target) as i64
            }
            // Trials of the God King: chapter per gate, DungeonIndex is the stage.
            ("ClearDungeon", "MILESTONE_MISSION_GOAL_7_1" | "MILESTONE_MISSION_GOAL_8_1" | "MILESTONE_MISSION_GOAL_9_1") => {
                let chapter = match goal { "MILESTONE_MISSION_GOAL_7_1" => 100000, "MILESTONE_MISSION_GOAL_8_1" => 100001, _ => 100002 };
                let target = required()?;
                self.cleared(|c, d| c == chapter && d >= target) as i64
            }
            // The Shadow of Aegina (chapter 50000): story stages 1-9, Devourer
            // Shakmeh 501-509 and Otherworldly Darkness Shakmeh 601-609.
            ("ClearDungeon", "MILESTONE_MISSION_GOAL_11" | "MILESTONE_MISSION_GOAL_12_1" | "MILESTONE_MISSION_GOAL_13_1") => {
                let base = match goal { "MILESTONE_MISSION_GOAL_11" => 0, "MILESTONE_MISSION_GOAL_12_1" => 500, _ => 600 };
                let target = required()?;
                self.cleared(|c, d| c == 50000 && d >= base + target && d <= base + 9) as i64
            }
            // Guideline tab.
            ("ClearDungeon", "MAIN_MISSION_GOAL_42") => progress(count("ClearDungeon", &[], &[27])),
            ("EnterDungeon", "MAIN_MISSION_GOAL_52") => progress(self.raid_metric(s, "EnterDungeon", &[1, 11], 0)),
            ("EnterDungeon", "MAIN_MISSION_GOAL_49") => progress(self.metric("EndEclipse", "all", None, None)),
            ("EnterDungeon", "MAIN_MISSION_GOAL_67") => progress(count("EnterDungeon", &[], &[24])),
            // Raid Shop (11) item 335 is the Baby Dragon's Kinship Gift Box,
            // 336 the Dragon's. The Hero's Inn shop (10) is the Stamina Shop.
            ("BuyShopItem", "MAIN_MISSION_GOAL_53_2") => progress(self.metric("BuyShopItem", "all", Some(11), Some(335))),
            ("BuyShopItem", "MAIN_MISSION_GOAL_61") => progress(self.metric("BuyShopItem", "all", Some(10), None)),
            ("BuyShopItem", "MILESTONE_MISSION_GOAL_112") => (self.metric("BuyShopItem", "all", Some(11), Some(335))
                + self.metric("BuyShopItem", "all", Some(11), Some(336)) > 0) as i64,
            ("BuyShopItem", "MILESTONE_MISSION_GOAL_104") => {
                let materials: std::collections::BTreeSet<i64> = s.tables.hero_shop.shop_items.iter()
                    .filter(|r| n(r, "ShopIndex") == 10 && r["ItemCode"].as_str().is_some_and(|c| c.starts_with("MATERIAL_PRESENT_")))
                    .map(|r| n(r, "Index")).collect();
                self.metrics_of("BuyShopItem").iter()
                    .any(|(_,k,shop,item,v)| k == "BuyShopItem" && *shop == 10 && *v > 0 && materials.contains(item)) as i64
            }
            ("CraftItem", "MAIN_MISSION_GOAL_37" | "MAIN_MISSION_GOAL_38" | "MILESTONE_MISSION_GOAL_56" | "MILESTONE_MISSION_GOAL_61" | "MILESTONE_MISSION_GOAL_71") => {
                let items: &[i64] = match goal {
                    "MAIN_MISSION_GOAL_37" => &[9999],                                   // Fragment of Infinity
                    "MAIN_MISSION_GOAL_38" => &[10000],                                  // Stone of Infinity
                    "MILESTONE_MISSION_GOAL_56" => &[2000, 2002],                        // (Random) Unique Weapon Ticket
                    "MILESTONE_MISSION_GOAL_61" => &[2015, 2017, 2018, 2019, 2036, 2061], // Unique Treasure Tickets
                    _ => &[3002],                                                        // Velkazar Artifact Ticket
                };
                let crafted: i64 = self.metrics_of("CraftItem").iter()
                    .filter(|(_,k,item,_,_)| k == "CraftItem" && items.contains(item)).map(|m| m.4).sum();
                if n(row, "ReqProgress") > 0 { crafted } else { (crafted > 0) as i64 }
            }
            // Growth tab.
            ("WearEquip", "MILESTONE_MISSION_GOAL_58") => equipped("mana_stone"),
            // Hero equip slots 8-10 hold the secondary Unique Treasures.
            ("WearEquip", "MILESTONE_MISSION_GOAL_62") => self.heroes().iter()
                .any(|h| (8..=10).any(|slot| n(h, &format!("EquipItemSlotIndex{slot}")) > 0)) as i64,
            ("ClearDungeon", "MILESTONE_MISSION_GOAL_95") => progress(self.metrics_of("ClearDungeon").iter()
                .filter(|(_,k,c,d,_)| k == "ClearDungeon" && self.tower_floor(s, 21, *c, *d).is_some())
                .map(|m| m.4).sum()),
            // Guild tab. Beast of Chaos (Manticore) gear is equipment set 102.
            ("WearEquip", "MILESTONE_MISSION_GOAL_80") => {
                let chaos: std::collections::BTreeSet<i64> = s.tables.extensions.rows("EquipItem").iter()
                    .filter(|r| n(r, "SetIndex") == 102).map(|r| n(r, "ItemIndex")).collect();
                self.equipment().iter().any(|e| chaos.contains(&n(e, "ItemIndex")) && n(e, "InventoryType") == 0
                    && self.equipped_slots().contains(&n(e, "SlotIndex"))) as i64
            }
            ("AddItem", "MILESTONE_MISSION_GOAL_102") => progress(self.metric("GuildSuppressRank", "all", Some(1), None)),
            ("ConsumeStamina", "MILESTONE_MISSION_GOAL_74") => progress(self.hist(super::state::part::GUILD_WINS).guild_attacks),
            // Raid tab (mission category 5). Goals name one raid and either a
            // stage ("Clear Fire Dragon Raid T8 Stage {1}") or a clear count.
            ("ClearDungeon", _) if raid_goal(goal).is_some() => {
                let (raids, base) = raid_goal(goal)?;
                match required() {
                    // Counted goals ("Clear Lava Raid {1} time(s)") report progress.
                    Some(_) if n(row,"ReqProgress") > 1 => self.raid_clears(s, raids, 0),
                    // Stage goals: any clear of that stage or a harder one.
                    Some(stage) => (self.raid_clears(s, raids, base + stage) > 0) as i64,
                    None => return None,
                }
            }
            // "Complete Challenge Raid Multi/Single Season {1} time(s)": each row
            // counts its own mode (Multi battle type 30, Single 25) and its nodes
            // are sequential blocks: node k (Variable1) counts the clears after
            // the previous nodes' {1} each, so Single 2 needs clears 6-10.
            ("ClearDungeon", "MILESTONE_MISSION_GOAL_31_1" | "MILESTONE_MISSION_GOAL_116_1") => {
                let per_node = required()?;
                let node = row["Variable1"].as_array()?.first()?.as_str()?.parse::<i64>().ok().filter(|v| *v > 0)?;
                let multi = goal == "MILESTONE_MISSION_GOAL_31_1";
                (count("ClearDungeon", &[], if multi { &[30] } else { &[25] }) - per_node * (node - 1)).max(0)
            }
            ("Enchant", "MILESTONE_MISSION_GOAL_16" | "MILESTONE_MISSION_GOAL_19" | "MILESTONE_MISSION_GOAL_22" | "MILESTONE_MISSION_GOAL_25") => {
                let prefix = match goal {
                    "MILESTONE_MISSION_GOAL_16" => "ENCHANTSCROLL_RED_",
                    "MILESTONE_MISSION_GOAL_19" => "ENCHANTSCROLL_BLUE_",
                    "MILESTONE_MISSION_GOAL_22" => "ENCHANTSCROLL_GREEN_",
                    _ => "ENCHANTSCROLL_BLACK_",
                };
                let scrolls: std::collections::BTreeSet<i64> = s.tables.extensions.rows("EquipEnchantOptionItem").iter()
                    .filter(|r| r["ItemCode"].as_str().is_some_and(|c| c.starts_with(prefix)))
                    .map(|r| n(r, "ItemIndex")).collect();
                self.metrics_of("Enchant").iter()
                    .filter(|(_,k,_,scroll,_)| k == "Enchant" && scrolls.contains(scroll))
                    .map(|m| m.4).sum()
            }
            // No verified filter/target: do not award completion from unrelated
            // clears, item grants, stamina spending, purchases or crafting.
            _ => return None,
        })
    }

    /// Clears of the given raids at `min_level` or above.
    fn raid_clears(&self, s: &AppState, raids: &[i64], min_level: i64) -> i64 {
        self.raid_metric(s, "ClearDungeon", raids, min_level)
    }

    /// `metric` (ClearDungeon or EnterDungeon) over the stages of the given
    /// raids at `min_level` or above. A raid is named by its shared index: the
    /// multi row's Index, which single/hard variants report as ClearRaidIndex.
    fn raid_metric(&self, s: &AppState, metric: &str, raids: &[i64], min_level: i64) -> i64 {
        let stages: std::collections::BTreeSet<(i64, i64)> = s.tables.battle.rows("Raid").iter()
            .filter(|r| {
                let shared = if n(r, "ClearRaidIndex") > 0 { n(r, "ClearRaidIndex") } else { n(r, "Index") };
                raids.contains(&shared) && n(r, "Level") >= min_level
            })
            .map(|r| (n(r, "ChapterIndex"), n(r, "DungeonIndex")))
            .collect();
        self.metrics_of(metric).iter()
            .filter(|(_,k,c,d,v)| k == metric && *v > 0 && stages.contains(&(*c, *d)))
            .map(|m| m.4).sum()
    }

    /// Whether any recorded clear matches (chapter, dungeon).
    fn cleared(&self, matches: impl Fn(i64, i64) -> bool) -> bool {
        self.metrics_of("ClearDungeon").iter()
            .any(|(_,k,c,d,v)| k == "ClearDungeon" && *v > 0 && matches(*c, *d))
    }
}

/// TowerType.Maze towers: the class Royal Underground Labyrinths.
const LABYRINTHS: std::ops::RangeInclusive<i64> = 1001..=1007;

/// Raid-tab goal -> (shared raid indices, Raid Level offset of stage 1 minus one).
/// Dragon "T8 Stage N" is Level 10+N; every other raid's Stage N is Level N.
/// Field raids keep both the current (21-24) and the retired (31-34) rows.
fn raid_goal(goal: &str) -> Option<(&'static [i64], i64)> {
    Some(match goal {
        "MILESTONE_MISSION_GOAL_14" => (&[1], 10),
        "MILESTONE_MISSION_GOAL_17" => (&[2], 10),
        "MILESTONE_MISSION_GOAL_20" => (&[3], 10),
        "MILESTONE_MISSION_GOAL_23" => (&[4], 10),
        "MILESTONE_MISSION_GOAL_15" => (&[11], 0),
        "MILESTONE_MISSION_GOAL_18" => (&[12], 0),
        "MILESTONE_MISSION_GOAL_21" => (&[13], 0),
        "MILESTONE_MISSION_GOAL_24" => (&[14], 0),
        // Field raids; the MAIN_ goals are the Guideline "Clear Field Raid X {1} time(s)".
        "MILESTONE_MISSION_GOAL_27" | "MILESTONE_MISSION_GOAL_106" | "MAIN_MISSION_GOAL_58" => (&[21, 31], 0),
        "MILESTONE_MISSION_GOAL_28" | "MILESTONE_MISSION_GOAL_107" | "MAIN_MISSION_GOAL_62" => (&[22, 32], 0),
        "MILESTONE_MISSION_GOAL_29" | "MILESTONE_MISSION_GOAL_108" | "MAIN_MISSION_GOAL_63" => (&[23, 33], 0),
        "MILESTONE_MISSION_GOAL_30" | "MILESTONE_MISSION_GOAL_109" | "MAIN_MISSION_GOAL_64" => (&[24, 34], 0),
        "MILESTONE_MISSION_GOAL_122" => (&[201], 0),
        "MILESTONE_MISSION_GOAL_123" => (&[202], 0),
        "MILESTONE_MISSION_GOAL_124" => (&[203], 0),
        "MISSION_GOAL_1002890" => (&[401], 0),
        "MISSION_GOAL_1002920" => (&[402], 0),
        _ => return None,
    })
}
