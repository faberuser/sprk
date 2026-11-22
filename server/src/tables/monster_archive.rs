//! Archive monsters per dungeon for the Monster Index, generated from the
//! client's wave and creature tables by
//! `server/scripts/tables/build_monster_archive.py`.
use std::collections::{BTreeMap, HashMap};
use std::path::Path;

/// One archive entry: (NShared.ArchiveType, CreatureTable index).
pub type ArchiveMonster = (i64, i64);

#[derive(Debug, Clone, Default)]
struct DungeonMonsters {
    by_difficulty: BTreeMap<i64, Vec<ArchiveMonster>>,
    scenario: Vec<ArchiveMonster>,
}

#[derive(Debug, Clone, Default)]
pub struct MonsterArchiveTable {
    /// PlayerArchive MainIndex of the Monster rows.
    pub main_index: i64,
    dungeons: HashMap<(i64, i64), DungeonMonsters>,
}

#[derive(serde::Deserialize)]
#[serde(rename_all = "PascalCase")]
struct File {
    archive_main_index: i64,
    dungeons: HashMap<String, HashMap<String, Vec<ArchiveMonster>>>,
}

impl MonsterArchiveTable {
    pub fn load(path: &Path) -> Result<Self, Box<dyn std::error::Error>> {
        let file: File = serde_json::from_reader(std::io::BufReader::new(std::fs::File::open(path)?))?;
        let mut dungeons = HashMap::new();
        for (key, sets) in file.dungeons {
            let (c, d) = key.split_once(':').ok_or_else(|| format!("Invalid dungeon key {key}"))?;
            let mut entry = DungeonMonsters::default();
            for (set, monsters) in sets {
                if set == "Scenario" {
                    entry.scenario = monsters;
                } else {
                    entry.by_difficulty.insert(set.parse()?, monsters);
                }
            }
            dungeons.insert((c.parse()?, d.parse()?), entry);
        }
        Ok(Self { main_index: file.archive_main_index, dungeons })
    }

    /// The archive monsters a battle of this dungeon contains. A difficulty
    /// the dungeon has no waves for falls back to its lowest one, as the
    /// client's scenario lookup does.
    pub fn monsters(&self, chapter: i64, dungeon: i64, difficulty: i64, scenario: bool) -> &[ArchiveMonster] {
        let Some(entry) = self.dungeons.get(&(chapter, dungeon)) else { return &[] };
        if scenario && !entry.scenario.is_empty() {
            return &entry.scenario;
        }
        entry.by_difficulty.get(&difficulty)
            .or_else(|| entry.by_difficulty.values().next())
            .map(Vec::as_slice)
            .unwrap_or(&[])
    }
}
