using System;
using System.Collections.Generic;
using NShared.NUtil;
using NVespa;

namespace NShared;

[JsonSerializable]
public class BattleCreatureCreateInfo
{
	public class BuffSkillInfo
	{
		public GuildSkillDataBase[] GuildSkillDataBases { get; set; }

		public AccountBuffDataBase[] AccountBuffDataBases { get; set; }

		public ClassBuffDataBase[] ClassBuffDataBasess { get; set; }

		public GuildArenaBuffDataBase[] GuildArenaBuffDatasBases { get; set; }

		public OrdealArenaSkillDataBase[] OrdealArenaSkillDataBases { get; set; }
	}

	public class BuffStatInfo
	{
		public DefaultExtraStatData[] ExtraStatDatas { get; set; }

		public OrdealArenaStatDataBase[] OrdealArenaStatDataBases { get; set; }

		public PetStatDataBase[] PetStatDataBases { get; set; }
	}

	public bool ForceNoDie;

	private long _wavePositionScore = -1L;

	public CachedHeroInfo HeroInfo { get; set; }

	public StatData StatData { get; set; }

	public Vector3 Position { get; set; }

	public int Tier { get; set; }

	public MonsterGradeType Grade { get; set; }

	public bool IsLeader { get; set; }

	public bool IsBoss { get; set; }

	public bool IsAi { get; set; }

	public bool IsSmartAi { get; set; }

	public bool IsPlayer { get; set; }

	public bool IsDummy { get; set; }

	public bool IsInitialResting { get; set; }

	public int RandomSeed { get; set; }

	public int WaveSlotIndex { get; set; }

	public string VisualInfo { get; set; }

	public int CreatureScaleR { get; set; }

	public int CreatureHeightMm { get; set; }

	public int OffsetMx { get; set; }

	public int OffsetMz { get; set; }

	public int DelayMs { get; set; }

	public ExtraSkillInfo[] AddExtraSkill { get; set; }

	public int WeaponIndex { get; set; }

	public GuildSkillDataBase[] GuildSkills { get; set; }

	public AccountBuffDataBase[] AccountBuffs { get; set; }

	public ClassBuffDataBase[] ClassBuffDataBases { get; set; }

	public GuildArenaBuffDataBase[] GuildArenaBuffDatasBases { get; set; }

	public OrdealArenaSkillDataBase[] OrdealArenaSkills { get; set; }

	public OrdealArenaStatDataBase[] OrdealArenaStats { get; set; }

	public PetStatDataBase[] PetStats { get; set; }

	public DefaultExtraStatData[] ExtraStats { get; set; }

	public int GroupIndex { get; set; }

	public int FirstSkillSlotIndex { get; set; }

	public bool HasDungeonSkill { get; set; }

	public long ForceMinHp { get; set; }

	public static Tuple<string, int, int> GetCompareResult(BattleCreatureCreateInfo l, BattleCreatureCreateInfo r)
	{
		Tuple<string, int, int> integrityCheckResult = NShared.HeroInfo.GetIntegrityCheckResult(l.HeroInfo, r.HeroInfo);
		if (integrityCheckResult != null && integrityCheckResult.Item1.Length > 0)
		{
			return new Tuple<string, int, int>(integrityCheckResult.Item1, integrityCheckResult.Item2, integrityCheckResult.Item3);
		}
		string diffLog = null;
		if (!l.StatData.StatContainer.EqualStat(r.StatData.StatContainer, out diffLog, out long diffLValue, out long diffRValue))
		{
			return new Tuple<string, int, int>("StatDiff_" + diffLog, (int)diffLValue, (int)diffRValue);
		}
		if (l.Tier != r.Tier)
		{
			return new Tuple<string, int, int>("Tier", l.Tier, r.Tier);
		}
		if (l.Grade != r.Grade)
		{
			return new Tuple<string, int, int>("Grade", (int)l.Grade, (int)r.Grade);
		}
		if (l.IsLeader != r.IsLeader)
		{
			return new Tuple<string, int, int>("IsLeader", 0, 0);
		}
		if (l.IsBoss != r.IsBoss)
		{
			return new Tuple<string, int, int>("IsBoss", 0, 0);
		}
		if (l.IsAi != r.IsAi)
		{
			return new Tuple<string, int, int>("IsAi", 0, 0);
		}
		if (l.IsPlayer != r.IsPlayer)
		{
			return new Tuple<string, int, int>("IsPlayer", 0, 0);
		}
		if (l.IsDummy != r.IsDummy)
		{
			return new Tuple<string, int, int>("IsDummy", 0, 0);
		}
		if (l.IsInitialResting != r.IsInitialResting)
		{
			return new Tuple<string, int, int>("IsInitialResting", 0, 0);
		}
		if (l.RandomSeed != r.RandomSeed)
		{
			return new Tuple<string, int, int>("RandomSeed", l.RandomSeed, r.RandomSeed);
		}
		if (l.WaveSlotIndex != r.WaveSlotIndex)
		{
			return new Tuple<string, int, int>("WaveSlotIndex", l.WaveSlotIndex, r.WaveSlotIndex);
		}
		if (l.OffsetMx != r.OffsetMx)
		{
			return new Tuple<string, int, int>("OffsetMx", l.OffsetMx, r.OffsetMx);
		}
		if (l.OffsetMz != r.OffsetMz)
		{
			return new Tuple<string, int, int>("OffsetMz", l.OffsetMz, r.OffsetMz);
		}
		if (l.DelayMs != r.DelayMs)
		{
			return new Tuple<string, int, int>("DelayMs", l.DelayMs, r.DelayMs);
		}
		if (l.HasDungeonSkill != r.HasDungeonSkill)
		{
			return new Tuple<string, int, int>("HasDungeonSkill", 0, 0);
		}
		int num = ((l.AddExtraSkill != null) ? l.AddExtraSkill.Length : 0);
		int num2 = ((r.AddExtraSkill != null) ? r.AddExtraSkill.Length : 0);
		if (num != num2)
		{
			return new Tuple<string, int, int>("AddExtraSkillCount", num, num2);
		}
		for (int i = 0; i < num; i++)
		{
			if (l.AddExtraSkill[i].SkillIndex != r.AddExtraSkill[i].SkillIndex)
			{
				return new Tuple<string, int, int>("AddExtraSkillIndexDiff_" + i, l.AddExtraSkill[i].SkillIndex, r.AddExtraSkill[i].SkillIndex);
			}
			if (l.AddExtraSkill[i].SkillLevel != r.AddExtraSkill[i].SkillLevel)
			{
				return new Tuple<string, int, int>("AddExtraSkillLevelDiff_" + i, l.AddExtraSkill[i].SkillLevel, r.AddExtraSkill[i].SkillLevel);
			}
		}
		return null;
	}

	public long GetWavePositionScore()
	{
		if (_wavePositionScore >= 0)
		{
			return _wavePositionScore;
		}
		if (HeroInfo == null)
		{
			VespaDebug.LogError("Cannot calculate position score. HeroInfo=null");
			return 0L;
		}
		_wavePositionScore = StatCalculator.GetCreatureWavePositionScore(HeroInfo.HeroIndex, StatData);
		return _wavePositionScore;
	}

	public static BattleCreatureCreateInfo CreateMyCreatureCreateInfo(CachedHeroInfo heroInfo, bool isLeader, bool isAi, BuffSkillInfo buffSkillInfo, BuffStatInfo buffStatInfo, int groupIndex = 0)
	{
		if (heroInfo == null)
		{
			return null;
		}
		StatData equippedHeroStat = StatCalculator.GetEquippedHeroStat(heroInfo);
		if (equippedHeroStat == null)
		{
			return null;
		}
		Random random = new Random(DateTime.UtcNow.Millisecond);
		return new BattleCreatureCreateInfo
		{
			IsAi = isAi,
			IsSmartAi = isAi,
			HeroInfo = heroInfo,
			StatData = equippedHeroStat,
			IsLeader = isLeader,
			IsPlayer = true,
			GroupIndex = groupIndex,
			AddExtraSkill = GetAddExtraSkill(heroInfo),
			RandomSeed = random.Next(),
			GuildSkills = buffSkillInfo?.GuildSkillDataBases,
			AccountBuffs = buffSkillInfo?.AccountBuffDataBases,
			ClassBuffDataBases = buffSkillInfo?.ClassBuffDataBasess,
			OrdealArenaSkills = buffSkillInfo?.OrdealArenaSkillDataBases,
			OrdealArenaStats = buffStatInfo?.OrdealArenaStatDataBases,
			PetStats = buffStatInfo?.PetStatDataBases,
			ExtraStats = buffStatInfo?.ExtraStatDatas
		};
	}

	public static BattleCreatureCreateInfo Create(CampaignWaveData.CreatureDetailInfo creatureInfo, int randomSeed)
	{
		if (creatureInfo == null)
		{
			return null;
		}
		ExtraSkillInfo[] array = null;
		if (creatureInfo.AddExtraSkill != null && creatureInfo.AddExtraSkill.Length != 0)
		{
			array = new ExtraSkillInfo[creatureInfo.AddExtraSkill.Length];
			for (int i = 0; i < creatureInfo.AddExtraSkill.Length; i++)
			{
				array[i] = new ExtraSkillInfo(creatureInfo.AddExtraSkill[i]);
			}
		}
		BattleCreatureCreateInfo battleCreatureCreateInfo = Create(creatureInfo.Index, creatureInfo.Star, creatureInfo.Level, randomSeed, creatureInfo.Tier, creatureInfo.Grade, creatureInfo.SkillOpen, 1000, creatureInfo.WeaponIndex);
		battleCreatureCreateInfo.IsBoss = creatureInfo.IsBoss;
		battleCreatureCreateInfo.WaveSlotIndex = creatureInfo.WaveSlotIndex;
		battleCreatureCreateInfo.VisualInfo = creatureInfo.VisualInfo;
		battleCreatureCreateInfo.OffsetMx = creatureInfo.OffsetMx;
		battleCreatureCreateInfo.OffsetMz = creatureInfo.OffsetMz;
		battleCreatureCreateInfo.DelayMs = creatureInfo.DelayMs;
		battleCreatureCreateInfo.CreatureScaleR = creatureInfo.ScaleR;
		battleCreatureCreateInfo.CreatureHeightMm = creatureInfo.HeightMm;
		battleCreatureCreateInfo.AddExtraSkill = array;
		battleCreatureCreateInfo.IsInitialResting = false;
		battleCreatureCreateInfo.ForceMinHp = creatureInfo.ForceMinHp;
		return battleCreatureCreateInfo;
	}

	public static BattleCreatureCreateInfo Create(int creatureIndex, int star, int level, int randomSeed, int tier, MonsterGradeType grade, int skillOpen, int statRatio = 1000, int weaponIndex = 0, bool useSoulWeapon = false)
	{
		BattleCreatureCreateInfo battleCreatureCreateInfo = new BattleCreatureCreateInfo();
		battleCreatureCreateInfo.HeroInfo = new CachedHeroInfo
		{
			HeroIndex = creatureIndex,
			Star = (sbyte)star,
			Level = (short)level,
			SkillLevel1 = (short)((skillOpen == 0 || skillOpen >= 1) ? level : 0),
			SkillLevel2 = (short)((skillOpen == 0 || skillOpen >= 2) ? level : 0),
			SkillLevel3 = (short)((skillOpen == 0 || skillOpen >= 3) ? level : 0),
			SkillLevel4 = (short)((skillOpen == 0 || skillOpen >= 4) ? level : 0),
			SkillLevel5 = (short)((skillOpen == 0 || skillOpen >= 5) ? level : 0)
		};
		StatType[] excludeLevesStatFactorTypes = new StatType[2]
		{
			StatType.PhysicalDefense,
			StatType.MagicalDefense
		};
		battleCreatureCreateInfo.IsAi = true;
		battleCreatureCreateInfo.IsBoss = false;
		battleCreatureCreateInfo.StatData = GameRoot.Instance.CreatureStatDataContainer.GetStatData(creatureIndex, star, level, 0, null, excludeLevesStatFactorTypes);
		battleCreatureCreateInfo.RandomSeed = randomSeed;
		battleCreatureCreateInfo.Tier = tier;
		battleCreatureCreateInfo.Grade = grade;
		if (tier > 0)
		{
			MonsterTierData tierData = GameRoot.Instance.MonsterTierDataContainer.GetData(tier);
			if (tierData != null && battleCreatureCreateInfo.StatData != null)
			{
				battleCreatureCreateInfo.StatData.ModifyStat(StatType.PhysicalAttack, (long stat) => CalculateDecimal(stat, tierData.PhysicalAttack, statRatio));
				battleCreatureCreateInfo.StatData.ModifyStat(StatType.MagicalAttack, (long stat) => CalculateDecimal(stat, tierData.MagicalAttack, statRatio));
				battleCreatureCreateInfo.StatData.ModifyStat(StatType.PhysicalDefense, (long stat) => CalculateDecimal(stat, tierData.PhysicalDefense, statRatio));
				battleCreatureCreateInfo.StatData.ModifyStat(StatType.MagicalDefense, (long stat) => CalculateDecimal(stat, tierData.MagicalDefense, statRatio));
				battleCreatureCreateInfo.StatData.ModifyStat(StatType.MaxHp, (long stat) => CalculateDecimal(stat, tierData.MaxHp, statRatio));
				battleCreatureCreateInfo.StatData.ModifyStat(StatType.MaxMp, (long stat) => CalculateDecimal(stat, tierData.MaxMp, statRatio));
				battleCreatureCreateInfo.StatData.ModifyStat(StatType.AntiCcChance, (long stat) => CalculateDecimal(stat, tierData.AntiCcChance, statRatio));
			}
		}
		battleCreatureCreateInfo.WeaponIndex = weaponIndex;
		if (weaponIndex > 0)
		{
			EquipItemInfo equipItemInfo = new EquipItemInfo
			{
				ItemIndex = weaponIndex,
				SlotIndex = EquipPartType.Weapon.GetEquipSlotIndex()
			};
			if (useSoulWeapon)
			{
				equipItemInfo.SetSoulWeaponInfo(new SoulWeaponInfo
				{
					EquipItemSlotIndex = equipItemInfo.SlotIndex,
					ItemIndex = weaponIndex
				});
			}
			battleCreatureCreateInfo.HeroInfo.SetEquipItemInfo(equipItemInfo.SlotIndex, equipItemInfo);
		}
		return battleCreatureCreateInfo;
	}

	private static long CalculateDecimal(long stat, long tierStat, int statRatio)
	{
		return Convert.ToInt64((decimal)stat * (decimal)tierStat * (decimal)statRatio / 1000m / 1000m);
	}

	public static ExtraSkillInfo[] GetAddExtraSkill(CachedHeroInfo heroInfo)
	{
		List<ExtraSkillInfo> list = new List<ExtraSkillInfo>();
		List<EquipOptionCreatedData> list2 = new List<EquipOptionCreatedData>();
		heroInfo.GetEquipOptionDataList(list2, null, list2);
		for (int i = 0; i < list2.Count; i++)
		{
			EquipOptionCreatedData equipOptionCreatedData = list2[i];
			if (equipOptionCreatedData != null && equipOptionCreatedData.OptionType == OperationType.UseSkill)
			{
				list.Add(new ExtraSkillInfo(equipOptionCreatedData.OptionValue));
			}
		}
		heroInfo.GetSetOptionList(out var openedOptionDataList, out var _);
		if (openedOptionDataList != null)
		{
			for (int j = 0; j < openedOptionDataList.Count; j++)
			{
				EquipOptionData equipOptionData = openedOptionDataList[j];
				if (equipOptionData != null && equipOptionData.OptionType == OperationType.UseSkill)
				{
					int optionValueMin = equipOptionData.GetOptionValueMin(0);
					list.Add(new ExtraSkillInfo(optionValueMin));
				}
			}
		}
		List<HeroRunePageInfo> runePageInfos = heroInfo.GetRunePageInfos(heroInfo.ApplyRunePage);
		if (runePageInfos != null)
		{
			foreach (HeroRunePageInfo item in runePageInfos)
			{
				PunishmentRuneOptionInfo punishmentRuneOptionInfo = heroInfo.GetPunishmentRuneOptionInfo(item);
				if (punishmentRuneOptionInfo != null)
				{
					for (int k = 1; k <= 3; k++)
					{
						int extraSkillIndex = punishmentRuneOptionInfo.GetExtraSkillIndex(k);
						int extraSkillValue = punishmentRuneOptionInfo.GetExtraSkillValue(k);
						list.Add(new ExtraSkillInfo(extraSkillIndex, extraSkillValue));
					}
				}
			}
		}
		int flaskItemIndex = heroInfo.FlaskItemIndex;
		if (flaskItemIndex != 0)
		{
			ItemData<FlaskItemData> data = GameRoot.Instance.ItemDataContainer.GetData<FlaskItemData>(flaskItemIndex);
			if (data != null)
			{
				int[] effectiveSkillIndex = data.SubData.EffectiveSkillIndex;
				if (effectiveSkillIndex != null && effectiveSkillIndex.Length != 0)
				{
					list.AddRangeFromIndex(effectiveSkillIndex);
				}
			}
		}
		return list.ToArray();
	}
}
