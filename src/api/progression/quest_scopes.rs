//! Restore known server-only mission filters without treating missing data as
//! a wildcard. Goal keys are stable identifiers from the archived localization;
//! display variables are used only where the goal defines their exact meaning.
use super::*;
impl View {
    pub(super) fn scoped_quest(&self, s: &AppState, row: &Value) -> i64 {
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
        match (kind, goal) {
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
                    .filter_map(|(_,_,c,d,_)|self.primary_tower_floor(s,tower,*c,*d))
                    .max().unwrap_or(0);
                required().is_some_and(|target| floor>=target) as i64
            }
            ("ClearDungeon", "MILESTONE_MISSION_GOAL_118") => progress(self.metric("CompleteWorldBoss","all",None,None)),
            ("EnterDungeon", "SUPERPASS_MISSION_GOAL_KASEL_12") => progress(count("EnterDungeon", &[], &[16])),
            ("EnterDungeon", "MAIN_MISSION_GOAL_55_1") => progress(count("EnterDungeon", &[], &[25,30])),
            ("ClearDungeon" | "EnterDungeon", "MAIN_MISSION_GOAL_47" | "SPECIAL_MISSION_GOAL_5000970") => progress(count(if kind=="ClearDungeon" {"ClearDungeon"} else {"EnterDungeon"}, &[], &[23])),
            ("BuyShopItem", "MILESTONE_MISSION_GOAL_79") => progress(self.metric("BuyShopItem","all",Some(20),None)),
            ("BuyShopItem", "ESTELLE_SPECIAL_MISSION_GOAL_5001990") => progress(self.metric("BuyShopItem","all",Some(21),None)),
            ("AwakenGear", "SUPERPASS_MISSION_GOAL_AWAKENGEAR_01") => self.metric("AwakenGear","all",None,None),
            // No verified filter/target: do not award completion from unrelated
            // clears, item grants, stamina spending, purchases or crafting.
            _ => 0,
        }
    }
}
