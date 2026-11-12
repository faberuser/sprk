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
        let count = |metric: &str, chapters: &[i64], types: &[i64]| self.metrics.iter()
            .filter(|(_,k,c,d,_)| k == metric && (chapters.is_empty() || chapters.contains(c))
                && (types.is_empty() || self.battle_type(s,*c,*d).is_some_and(|t|types.contains(&t))))
            .map(|v|v.4).sum::<i64>();
        let progress = |value: i64| {
            if n(row,"ReqProgress") > 0 { value }
            else { required().is_some_and(|target| value >= target) as i64 }
        };
        let equipment_matches = |item: i64, class: &str| {
            let Some((part,detail,equip_type))=self.equipment_metadata(s,item) else {return false;};
            match class {
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
        let equipped = |class: &str| self.equipment.iter().any(|e|
            equipment_matches(n(e,"ItemIndex"),class) && n(e,"InventoryType")==0 &&
            self.equipped_slots.contains(&n(e,"SlotIndex"))) as i64;
        let action_count = |action: &str, class: &str| self.metrics.iter()
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
                (self.equipment.iter().any(|e|equipment_matches(n(e,"ItemIndex"),class) && n(e,"Star")>=target)
                    || self.metrics.iter().any(|(_,k,item,star,v)|k=="AwakenGear" && *v>0 && *star>=target && equipment_matches(*item,class))) as i64
            }
            ("AwakenGear", "MAIN_MISSION_GOAL_25") => {
                let target=required()?;
                self.metrics.iter().any(|(_,k,item,star,v)|k=="AwakenGear" && *item>0 && *star>=target && *v>0) as i64
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
            ("UseItem", "MAIN_MISSION_GOAL_11") => self.metrics.iter().filter(|(_,k,item,_,_)|
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
                required().is_some_and(|target| self.guild_damage.get("raid").copied().unwrap_or(0) >= target) as i64
            }
            ("ClearDungeon", "MAIN_MISSION_GOAL_11582") => progress(count("ClearDungeon", &[], &[40])),
            ("ClearDungeon", "MAIN_MISSION_GOAL_11791") => progress(count("ClearDungeon", &[], &[41])),
            ("ClearDungeon", "MAIN_MISSION_GOAL_11792") => progress(count("ClearDungeon", &[], &[42])),
            ("ClearDungeon", "MAIN_MISSION_GOAL_14" | "MILESTONE_MISSION_GOAL_2" | "SUPERPASS_MISSION_GOAL_FREY_08" | "SUPERPASS_MISSION_GOAL_KASEL_08"
                | "MAIN_MISSION_GOAL_34" | "MILESTONE_MISSION_GOAL_1") => {
                let tower = if matches!(goal,"MAIN_MISSION_GOAL_34"|"MILESTONE_MISSION_GOAL_1") {11} else {21};
                let floor = self.metrics.iter().filter(|(_,k,_,_,v)| k=="ClearDungeon" && *v>0)
                    .filter_map(|(_,_,c,d,_)|self.tower_floor(s,tower,*c,*d))
                    .max().unwrap_or(0);
                required().is_some_and(|target| floor>=target) as i64
            }
            ("ClearDungeon", "MILESTONE_MISSION_GOAL_118") => progress(self.metric("CompleteWorldBoss","all",None,None)),
            ("EnterDungeon", "SUPERPASS_MISSION_GOAL_KASEL_12") => progress(count("EnterDungeon", &[], &[16])),
            ("EnterDungeon", "MAIN_MISSION_GOAL_55_1") => progress(count("EnterDungeon", &[], &[25,30])),
            ("ClearDungeon" | "EnterDungeon", "MAIN_MISSION_GOAL_47" | "SPECIAL_MISSION_GOAL_5000970") => progress(count(if kind=="ClearDungeon" {"ClearDungeon"} else {"EnterDungeon"}, &[], &[23])),
            ("BuyShopItem", "MILESTONE_MISSION_GOAL_79") => progress(self.metric("BuyShopItem","all",Some(20),None)),
            ("BuyShopItem", "ESTELLE_SPECIAL_MISSION_GOAL_5001990") => progress(self.metric("BuyShopItem","all",Some(21),None)),
            ("BuyShopItem", "MILESTONE_MISSION_GOAL_76" | "MAIN_MISSION_GOAL_36_1" | "MILESTONE_MISSION_GOAL_26" | "MILESTONE_MISSION_GOAL_113" | "MAIN_MISSION_GOAL_31") => {
                let shop=match goal {"MILESTONE_MISSION_GOAL_76"=>27,"MAIN_MISSION_GOAL_36_1"=>12,"MILESTONE_MISSION_GOAL_26"=>11,"MILESTONE_MISSION_GOAL_113"=>15,_=>1};
                self.metric("BuyShopItem","all",Some(shop),None)
            }
            ("AwakenGear", "SUPERPASS_MISSION_GOAL_AWAKENGEAR_01") => self.metric("AwakenGearAttempt","all",None,None)
                + self.metric("AwakenGear","all",Some(0),None),
            // No verified filter/target: do not award completion from unrelated
            // clears, item grants, stamina spending, purchases or crafting.
            _ => return None,
        })
    }
}
