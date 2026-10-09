use serde::Deserialize;
use serde_json::Value;
use std::path::Path;

#[derive(Debug, Clone, Default, Deserialize)]
#[serde(rename_all = "PascalCase")]
pub struct ProgressionTable {
    pub achievements: Vec<Value>,
    pub sub_quests: Vec<Value>,
    pub main_quests: Vec<Value>,
    pub clear_missions: Vec<Value>,
    pub clear_products: Vec<Value>,
    pub newbie_missions: Vec<Value>,
    pub newbie_products: Vec<Value>,
    pub login_rewards: Vec<Value>,
    pub chapter_rewards: Vec<Value>,
    pub attendance_info: Vec<Value>,
    pub chapters: Vec<Value>,
    pub mission_categories: Vec<Value>,
    pub mission_visuals: Vec<Value>,
    #[serde(default)]
    pub calendars: Vec<Value>,
}
impl ProgressionTable {
    pub fn load(dir: &Path) -> Result<Self, Box<dyn std::error::Error>> {
        let mut data: Self =
            serde_json::from_reader(std::fs::File::open(dir.join("ProgressionSupport.json"))?)?;
        data.calendars =
            serde_json::from_reader(std::fs::File::open(dir.join("AttendanceCalendars.json"))?)?;
        let mut indices = std::collections::HashSet::new();
        for c in &data.calendars {
            let id = c["Index"].as_i64().ok_or("Calendar Index required")?;
            if id <= 0
                || !indices.insert(id)
                || !c["Reward"]
                    .as_array()
                    .is_some_and(|r| !r.is_empty() && r.len() <= 255)
            {
                return Err("Invalid attendance calendar".into());
            }
            if !data.attendance_info.iter().any(|r| r["Index"] == id) {
                return Err("Calendar Index has no client AttendanceInfoTable entry".into());
            }
            let start = chrono::NaiveDateTime::parse_from_str(
                c["StartDate"]
                    .as_str()
                    .ok_or("Calendar StartDate required")?,
                "%Y-%m-%d %H:%M:%S",
            )?;
            let end = chrono::NaiveDateTime::parse_from_str(
                c["EndDate"].as_str().ok_or("Calendar EndDate required")?,
                "%Y-%m-%d %H:%M:%S",
            )?;
            if start >= end
                || c["RewardEndDay"].as_u64() != Some(c["Reward"].as_array().unwrap().len() as u64)
            {
                return Err("Invalid calendar dates or RewardEndDay".into());
            }
            for field in ["ManualStart", "NextAttendanceIndex", "SendMail"] {
                if c[field].as_i64().unwrap_or(0) != 0 {
                    return Err(format!("Local calendars do not support {field}").into());
                }
            }
            for (day, reward) in c["Reward"].as_array().unwrap().iter().enumerate() {
                let entries = reward["Reward"]
                    .as_array()
                    .filter(|r| !r.is_empty())
                    .ok_or("Calendar day has no rewards")?;
                for r in entries {
                    if r["Index"] != id
                        || r["Day"].as_u64() != Some(day as u64 + 1)
                        || r["SendMail"] == true
                    {
                        return Err("Invalid calendar reward index/day/delivery".into());
                    }
                }
            }
        }
        Ok(data)
    }
}
