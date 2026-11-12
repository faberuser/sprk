using System;
using MessagePack;

namespace NShared;

[MessagePackObject(false)]
[JsonSerializable]
public class CampaignDungeonData : IDataKeySelector<ChapterDungeonKey2>
{
	ChapterDungeonKey2 IDataKeySelector<ChapterDungeonKey2>.Key => new ChapterDungeonKey2(ChapterIndex, DungeonIndex);

	[Key(0)]
	public int ChapterIndex { get; set; }

	[Key(1)]
	public int DungeonIndex { get; set; }

	[Key(2)]
	public HashString BattleField { get; set; }

	[Key(3)]
	public bool IsExceededEntryChapter { get; set; }

	[Key(4)]
	public BattleType BattleType { get; set; }

	[Key(5)]
	public HashString BattleDefineCode { get; set; }

	[Key(6)]
	public HashString BgmCode { get; set; }

	[Key(7)]
	public bool Scenario { get; set; }

	[Key(8)]
	public int ScenarioEndWave { get; set; }

	[Key(9)]
	public int[] ReqScenarioDungeon { get; set; }

	[Key(10)]
	public string[] Constant { get; set; }

	[Key(11)]
	public int[] ForcedHeroIndex { get; set; }

	[Key(12)]
	public int[] ScenarioForcedHeroIndex { get; set; }

	[Key(13)]
	public int[] RandomSkillProjectileGroup { get; set; }

	[Key(14)]
	public int FirstRewardIndex { get; set; }

	[Key(15)]
	public StaminaType ReqStaminaType { get; set; }

	[Key(16)]
	public int ReqStamina { get; set; }

	[Key(17)]
	public int AdvancedReqStamina { get; set; }

	[IgnoreMember]
	public sbyte DeckCount => (sbyte)(MainSquardCount + SubSquardCount);

	[Key(19)]
	public sbyte MainSquardCount { get; set; }

	[Key(20)]
	public sbyte SubSquardCount { get; set; }

	[Key(21)]
	public string LocalStorageKey { get; set; }

	[Key(22)]
	public int FirstDungeonIndex { get; set; }

	[Key(23)]
	public int NextDungeonIndex { get; set; }

	[Key(24)]
	public int DropRewardIndex_Easy { get; set; }

	[Key(25)]
	public int CreatureExp_Easy { get; set; }

	[Key(26)]
	public int FirstClearRewardIndex_Easy { get; set; }

	[Key(27)]
	public int FirstClearGem_Easy { get; set; }

	[Key(28)]
	public int ReqStamina_Easy { get; set; }

	[Key(29)]
	public DispatchType DispatchType_Easy { get; set; }

	[Key(30)]
	public int DropRewardIndex_Normal { get; set; }

	[Key(31)]
	public int CreatureExp_Normal { get; set; }

	[Key(32)]
	public int FirstClearRewardIndex_Normal { get; set; }

	[Key(33)]
	public int FirstClearGem_Normal { get; set; }

	[Key(34)]
	public int ReqStamina_Normal { get; set; }

	[Key(35)]
	public DispatchType DispatchType_Normal { get; set; }

	[Key(36)]
	public int DropRewardIndex_Hard { get; set; }

	[Key(37)]
	public int CreatureExp_Hard { get; set; }

	[Key(38)]
	public int FirstClearRewardIndex_Hard { get; set; }

	[Key(39)]
	public int FirstClearGem_Hard { get; set; }

	[Key(40)]
	public int ReqStamina_Hard { get; set; }

	[Key(41)]
	public DispatchType DispatchType_Hard { get; set; }

	[Key(42)]
	public int DropRewardIndex_Hell { get; set; }

	[Key(43)]
	public int CreatureExp_Hell { get; set; }

	[Key(44)]
	public int FirstClearRewardIndex_Hell { get; set; }

	[Key(45)]
	public int FirstClearGem_Hell { get; set; }

	[Key(46)]
	public int ReqStamina_Hell { get; set; }

	[Key(47)]
	public DispatchType DispatchType_Hell { get; set; }

	[Key(48)]
	public byte CheckClearChapter { get; set; }

	[Key(49)]
	public int[] Value { get; set; }

	[Key(50)]
	public bool OpenDifficulty { get; set; }

	[Key(51)]
	public bool IsBranch { get; set; }

	[Key(52)]
	public bool NoDifficulty { get; set; }

	[Key(53)]
	public EGuildFlagDisplayType EGuildFlagDisplayType { get; set; }

	[Key(54)]
	public int DispatchDeckCount { get; set; }

	[Key(55)]
	public int[] DungeonSkillIndexList { get; set; }

	[Key(56)]
	public string ScenarioBattleDefine { get; set; }

	[Key(57)]
	public int[] DungeonPassiveEnvironmentIndices { get; set; }

	[Key(58)]
	public bool IsNotInItDailyCount { get; set; }

	[Key(59)]
	public SkillAttrType[] DeckAttributeCondition { get; set; }

	[Key(61)]
	public int AffixGroupIndex_Easy { get; set; }

	[Key(62)]
	public int AffixGroupIndex_Normal { get; set; }

	[Key(63)]
	public int AffixGroupIndex_Hard { get; set; }

	[Key(64)]
	public int AffixGroupIndex_Hell { get; set; }

	[Key(65)]
	public int AffixMultiple_Easy { get; set; }

	[Key(66)]
	public int AffixMultiple_Normal { get; set; }

	[Key(67)]
	public int AffixMultiple_Hard { get; set; }

	[Key(68)]
	public int AffixMultiple_Hell { get; set; }

	[Key(69)]
	public int ClearPoint_Easy { get; set; }

	[Key(70)]
	public int ClearPoint_Normal { get; set; }

	[Key(71)]
	public int ClearPoint_Hard { get; set; }

	[Key(72)]
	public int ClearPoint_Hell { get; set; }

	[Key(73)]
	public SweepDungeonType SweepDungeonType { get; set; }

	[Key(74)]
	public int SweepTicketCount { get; set; }

	[Key(75)]
	public int SweepTicketMaxCount { get; set; }

	public BattleDefineData GetBattleDefine(bool isScenario)
	{
		if (isScenario)
		{
			if (!string.IsNullOrEmpty(ScenarioBattleDefine))
			{
				return GameRoot.Instance.BattleDefineDataContainer.GetData(ScenarioBattleDefine);
			}
			if (SubSquardCount > 0)
			{
				return GameRoot.Instance.BattleDefineDataContainer.GetData("Scenario_8");
			}
			return GameRoot.Instance.BattleDefineDataContainer.GetData("Scenario");
		}
		return GameRoot.Instance.BattleDefineDataContainer.GetData(BattleDefineCode);
	}

	public int GetStaminaOfDifficulty(SaveStaminaBoosterInfo boosterInfo = null, DungeonDifficultyType difficultyType = DungeonDifficultyType.Easy, bool includeAdvancedStamina = true)
	{
		int num = ReqStamina;
		if (difficultyType == DungeonDifficultyType.Hell && ReqStamina_Hell > 0)
		{
			num = ReqStamina_Hell;
		}
		if (difficultyType == DungeonDifficultyType.Hard && ReqStamina_Hard > 0)
		{
			num = ReqStamina_Hard;
		}
		if (difficultyType == DungeonDifficultyType.Normal && ReqStamina_Normal > 0)
		{
			num = ReqStamina_Normal;
		}
		if (difficultyType == DungeonDifficultyType.Easy && ReqStamina_Easy > 0)
		{
			num = ReqStamina_Easy;
		}
		if (includeAdvancedStamina)
		{
			num += AdvancedReqStamina;
		}
		if ((ReqStaminaType != StaminaType.Chicken && boosterInfo != null) || (boosterInfo != null && !boosterInfo.BattleTypeList.Contains(BattleType)))
		{
			boosterInfo.SaveStaminaValue = 0;
		}
		if (boosterInfo != null && boosterInfo.SaveStaminaValue > 0)
		{
			int num2 = (int)Math.Floor((float)(num * boosterInfo.SaveStaminaValue / 100));
			num -= num2;
		}
		if (num < 0)
		{
			num = 0;
		}
		return num;
	}

	public int GetRewardIndexOfDifficulty(DungeonDifficultyType difficultyType)
	{
		return difficultyType switch
		{
			DungeonDifficultyType.Easy => DropRewardIndex_Easy, 
			DungeonDifficultyType.Normal => DropRewardIndex_Normal, 
			DungeonDifficultyType.Hard => DropRewardIndex_Hard, 
			DungeonDifficultyType.Hell => DropRewardIndex_Hell, 
			_ => 0, 
		};
	}

	public int GetCreatureExpOfDifficulty(DungeonDifficultyType difficultyType)
	{
		return difficultyType switch
		{
			DungeonDifficultyType.Easy => CreatureExp_Easy, 
			DungeonDifficultyType.Normal => CreatureExp_Normal, 
			DungeonDifficultyType.Hard => CreatureExp_Hard, 
			DungeonDifficultyType.Hell => CreatureExp_Hell, 
			_ => 0, 
		};
	}

	public int[] GetForcedHeroIndex(bool isScenario)
	{
		if (isScenario && ScenarioForcedHeroIndex != null && ScenarioForcedHeroIndex.Length != 0)
		{
			return ScenarioForcedHeroIndex;
		}
		return ForcedHeroIndex;
	}

	public int GetRandomProjectileGroupIndex(int randomseed)
	{
		if (RandomSkillProjectileGroup != null && RandomSkillProjectileGroup.Length != 0)
		{
			int num = randomseed % RandomSkillProjectileGroup.Length;
			return RandomSkillProjectileGroup[num];
		}
		return -1;
	}

	public int GetAffixGroupIndex(DungeonDifficultyType difficultyType)
	{
		return difficultyType switch
		{
			DungeonDifficultyType.Easy => AffixGroupIndex_Easy, 
			DungeonDifficultyType.Normal => AffixGroupIndex_Normal, 
			DungeonDifficultyType.Hard => AffixGroupIndex_Hard, 
			DungeonDifficultyType.Hell => AffixGroupIndex_Hell, 
			_ => 0, 
		};
	}

	public int GetAffixMultiple(DungeonDifficultyType difficultyType)
	{
		return difficultyType switch
		{
			DungeonDifficultyType.Easy => AffixMultiple_Easy, 
			DungeonDifficultyType.Normal => AffixMultiple_Normal, 
			DungeonDifficultyType.Hard => AffixMultiple_Hard, 
			DungeonDifficultyType.Hell => AffixMultiple_Hell, 
			_ => 0, 
		};
	}

	public int GetClearPoint(DungeonDifficultyType difficultyType)
	{
		return difficultyType switch
		{
			DungeonDifficultyType.Easy => ClearPoint_Easy, 
			DungeonDifficultyType.Normal => ClearPoint_Normal, 
			DungeonDifficultyType.Hard => ClearPoint_Hard, 
			DungeonDifficultyType.Hell => ClearPoint_Hell, 
			_ => 0, 
		};
	}
}
