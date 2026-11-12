using System.Collections;
using System.Collections.Generic;
using MessagePack;
using NShared.NJit;
using NVespa;

namespace NShared;

public class CampaignWaveDataContainer : JitDataContainerDefault<CampaignWaveKey2, CampaignWaveData>
{
	[MessagePackObject(false)]
	public class ChapterDungeonInfo
	{
		[Key(0)]
		public DungeonDifficultyType MinDifficulty { get; set; }

		[Key(1)]
		public DungeonDifficultyType MaxDifficulty { get; set; }
	}

	public class WaveCollectionData
	{
		public DungeonDifficultyType MinDifficulty { get; private set; }

		public DungeonDifficultyType MaxDifficulty { get; private set; }

		public Dictionary<DungeonDifficultyType, Dictionary<int, CampaignWaveData>> DifficultyDic { get; private set; }

		public Dictionary<int, CampaignWaveData> ScenarioWaveDic { get; private set; }

		public WaveCollectionData(DungeonDifficultyType minDifficulty, DungeonDifficultyType maxDifficulty, Dictionary<DungeonDifficultyType, Dictionary<int, CampaignWaveData>> difficultyDic, Dictionary<int, CampaignWaveData> scenarioWaveDic)
		{
			MinDifficulty = minDifficulty;
			MaxDifficulty = maxDifficulty;
			DifficultyDic = difficultyDic;
			ScenarioWaveDic = scenarioWaveDic;
			if (DifficultyDic == null)
			{
				DifficultyDic = new Dictionary<DungeonDifficultyType, Dictionary<int, CampaignWaveData>>();
			}
		}
	}

	public const string TABLENAME = "CampaignWaveTable";

	private JitDictionary<ChapterDungeonKey2, ChapterDungeonInfo> _info_by_ChapterDungeon = new JitDictionary<ChapterDungeonKey2, ChapterDungeonInfo>();

	private JitDictionary<ChapterDungeonKey2, Dictionary<int, CampaignWaveData>> _scenarioWaveDic_by_ChapterDungeon = new JitDictionary<ChapterDungeonKey2, Dictionary<int, CampaignWaveData>>();

	private JitDictionary<ChapterDungeonKey2, Dictionary<DungeonDifficultyType, Dictionary<int, CampaignWaveData>>> _difficultyWaveDic_by_ChapterDungeon = new JitDictionary<ChapterDungeonKey2, Dictionary<DungeonDifficultyType, Dictionary<int, CampaignWaveData>>>();

	private Dictionary<ChapterDungeonKey2, WaveCollectionData> _cache_of_collection = new Dictionary<ChapterDungeonKey2, WaveCollectionData>();

	public override TablePathDefine PathDefine => new TablePathDefine("CampaignWaveTable");

	public CampaignWaveDataContainer()
	{
		base.Packer.RegisterAndBind(_info_by_ChapterDungeon);
		base.Packer.RegisterAndBind(_difficultyWaveDic_by_ChapterDungeon);
		base.Packer.RegisterAndBind(_scenarioWaveDic_by_ChapterDungeon);
	}

	protected override void OnLoadingValue_from_JsonData(IDictionary jsonDic, CampaignWaveData value)
	{
		CampaignWaveData.CreatureDetailWritable creatureDetailWritable = ReadCreature_from_JsonData(jsonDic, "BossCreatureIndex", "BossWeaponIndex", "Boss", string.Empty, value, isBoss: true, 0);
		CampaignWaveData.CreatureDetailWritable creatureDetailWritable2 = ReadCreature_from_JsonData(jsonDic, "BossCreatureIndex2", "BossWeaponIndex2", "Boss", "2", value, isBoss: true, 0);
		if (creatureDetailWritable != null || creatureDetailWritable2 != null)
		{
			value.BossList = new List<CampaignWaveData.CreatureDetailWritable>();
			if (creatureDetailWritable != null)
			{
				value.BossList.Add(creatureDetailWritable);
			}
			if (creatureDetailWritable2 != null)
			{
				value.BossList.Add(creatureDetailWritable2);
			}
		}
		for (int i = 1; i < 100; i++)
		{
			CampaignWaveData.CreatureDetailWritable creatureDetailWritable3 = ReadCreature_from_JsonData(jsonDic, "CreatureIndex" + i, "WeaponIndex" + i, "Creature", i.ToString(), value, isBoss: false, i);
			if (creatureDetailWritable3 != null)
			{
				value.CreatureList = value.CreatureList ?? new List<CampaignWaveData.CreatureDetailWritable>();
				value.CreatureList.Add(creatureDetailWritable3);
			}
		}
	}

	private static CampaignWaveData.CreatureDetailWritable ReadCreature_from_JsonData(IDictionary jsonDic, string keyword_of_Index, string keyword_of_WeaponIndex, string prefix_of_JsonParse, string postfix_of_JsonParse, CampaignWaveData waveData, bool isBoss, int waveSlotIndex)
	{
		if (!jsonDic.Contains(keyword_of_Index))
		{
			return null;
		}
		int num = JsonMarshalerUsingReflection.ParseOrDefaultPrimitive<int>(jsonDic, keyword_of_Index);
		if (num <= 0)
		{
			return null;
		}
		if (!(JsonMarshalerUsingReflection.Parse(typeof(CampaignWaveData.CreatureReadData), jsonDic, prefix_of_JsonParse, postfix_of_JsonParse) is CampaignWaveData.CreatureReadData creatureReadData))
		{
			return null;
		}
		int weaponIndex = JsonMarshalerUsingReflection.ParseOrDefaultPrimitive<int>(jsonDic, keyword_of_WeaponIndex);
		return new CampaignWaveData.CreatureDetailWritable(num, (creatureReadData.Star > 0) ? creatureReadData.Star : waveData.Star, (creatureReadData.Level > 0) ? creatureReadData.Level : waveData.Level, (creatureReadData.Tier > 0) ? creatureReadData.Tier : waveData.Tier, creatureReadData.Grade, isBoss, creatureReadData.Flag, waveSlotIndex, creatureReadData.VisualInfo, creatureReadData.OffsetMx, creatureReadData.OffsetMz, creatureReadData.DelayMs, creatureReadData.ScaleR, creatureReadData.HeightMm, (creatureReadData.SkillOpen > 0) ? creatureReadData.SkillOpen : waveData.SkillOpen, creatureReadData.AddExtraSkill, weaponIndex, creatureReadData.GroupIndex, creatureReadData.MinHp);
	}

	protected override void OnEndLoadFromJsonData()
	{
		foreach (CampaignWaveData item in base.BaseList.JsonLoad_GetOrNew())
		{
			OnAddData_from_JsonData(item);
		}
	}

	public CampaignWaveData GetData(int chapterIndex, int dungeonIndex, DungeonDifficultyType difficulty, bool scenario, int waveIndex)
	{
		if (!scenario)
		{
			return GetData(new CampaignWaveKey2(chapterIndex, dungeonIndex, difficulty, scenario, waveIndex));
		}
		CampaignDungeonData data = GameRoot.Instance.CampaignDungeonDataContainer.GetData(chapterIndex, dungeonIndex);
		if (data == null)
		{
			return null;
		}
		int num = ((data.ScenarioEndWave == 0) ? 9999 : data.ScenarioEndWave);
		if (waveIndex > num)
		{
			return null;
		}
		WaveCollectionData waveCollection = GetWaveCollection(chapterIndex, dungeonIndex);
		if (waveCollection == null)
		{
			return null;
		}
		CampaignWaveData value = null;
		if (waveCollection.ScenarioWaveDic != null && waveCollection.ScenarioWaveDic.TryGetValue(waveIndex, out value))
		{
			return value;
		}
		if (!waveCollection.DifficultyDic.TryGetValue(waveCollection.MinDifficulty, out var value2))
		{
			return null;
		}
		if (!value2.TryGetValue(waveIndex, out value))
		{
			return null;
		}
		return value;
	}

	public WaveCollectionData GetWaveCollection(int chapterIndex, int dungeonIndex)
	{
		lock (_cache_of_collection)
		{
			ChapterDungeonKey2 key = new ChapterDungeonKey2(chapterIndex, dungeonIndex);
			if (_cache_of_collection.TryGetValue(key, out var value))
			{
				return value;
			}
			ChapterDungeonInfo orDefault = _info_by_ChapterDungeon.GetOrDefault(key);
			Dictionary<DungeonDifficultyType, Dictionary<int, CampaignWaveData>> orDefault2 = _difficultyWaveDic_by_ChapterDungeon.GetOrDefault(key);
			Dictionary<int, CampaignWaveData> orDefault3 = _scenarioWaveDic_by_ChapterDungeon.GetOrDefault(key);
			if (orDefault2 == null && orDefault3 == null)
			{
				return null;
			}
			value = new WaveCollectionData(orDefault?.MinDifficulty ?? DungeonDifficultyType.Easy, orDefault?.MaxDifficulty ?? DungeonDifficultyType.Easy, orDefault2, orDefault3);
			_cache_of_collection.Add(key, value);
			return value;
		}
	}

	public int GetFirstBossIndexOfDungeon(int chapterIndex, int dungeonIndex, DungeonDifficultyType difficulty)
	{
		int num = 1;
		while (true)
		{
			CampaignWaveData data = GetData(chapterIndex, dungeonIndex, difficulty, scenario: false, num);
			if (data == null)
			{
				break;
			}
			if (data.BossList != null && data.BossList.Count != 0)
			{
				return data.BossList[0].Index;
			}
			num++;
		}
		return 0;
	}

	private void OnAddData_from_JsonData(CampaignWaveData value)
	{
		new ChapterDungeonKey2(value.ChapterIndex, value.DungeonIndex);
		if (value.Scenario)
		{
			OnAddData_Scenario_from_JsonData(value);
		}
		else
		{
			OnAddData_NoScenario_from_JsonData(value);
		}
	}

	private void OnAddData_Scenario_from_JsonData(CampaignWaveData value)
	{
		Dictionary<int, CampaignWaveData> dictionary = ___Shared_Jit_JitDictionary___.JsonLoad_ValueGetOrNew(key: new ChapterDungeonKey2(value.ChapterIndex, value.DungeonIndex), dic: _scenarioWaveDic_by_ChapterDungeon);
		if (dictionary.ContainsKey(value.WaveIndex))
		{
			VespaDebug.LogError($"Alread Inserted (scenario): Chapter={value.ChapterIndex}, Dungeon={value.DungeonIndex}, Difficulty={value.Difficulty}, WaveIndex={value.WaveIndex}");
		}
		else
		{
			dictionary.Add(value.WaveIndex, value);
		}
	}

	private void OnAddData_NoScenario_from_JsonData(CampaignWaveData value)
	{
		ChapterDungeonKey2 key = new ChapterDungeonKey2(value.ChapterIndex, value.DungeonIndex);
		ChapterDungeonInfo chapterDungeonInfo = _info_by_ChapterDungeon.JsonLoad_ValueGetOrNew(key);
		Dictionary<DungeonDifficultyType, Dictionary<int, CampaignWaveData>> dictionary = _difficultyWaveDic_by_ChapterDungeon.JsonLoad_ValueGetOrNew(key);
		if (dictionary.Count == 0)
		{
			chapterDungeonInfo.MinDifficulty = value.Difficulty;
			chapterDungeonInfo.MaxDifficulty = value.Difficulty;
		}
		else
		{
			if (chapterDungeonInfo.MinDifficulty > value.Difficulty)
			{
				chapterDungeonInfo.MinDifficulty = value.Difficulty;
			}
			if (chapterDungeonInfo.MaxDifficulty < value.Difficulty)
			{
				chapterDungeonInfo.MaxDifficulty = value.Difficulty;
			}
		}
		if (!dictionary.TryGetValue(value.Difficulty, out var value2))
		{
			value2 = new Dictionary<int, CampaignWaveData>();
			dictionary.Add(value.Difficulty, value2);
		}
		if (value2.ContainsKey(value.WaveIndex))
		{
			VespaDebug.LogError($"Alread Inserted (no scenario): Chapter={value.ChapterIndex}, Dungeon={value.DungeonIndex}, Difficulty={value.Difficulty}, WaveIndex={value.WaveIndex}");
		}
		else
		{
			value2.Add(value.WaveIndex, value);
		}
	}
}
