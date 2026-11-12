using System;
using System.Collections.Generic;
using NShared.NUtil;
using NVespa;

namespace NShared;

public class StatCalculator
{
	[Flags]
	public enum HeroStatCalculationFlag
	{
		None = 0,
		Equip = 1,
		EquipOption = 2,
		All = 0xFFFF
	}

	public static StatData GetHeroStat(HeroInfo heroInfo, StatType[] filteredTypes = null)
	{
		return GameRoot.Instance.CreatureStatDataContainer.GetStatData(heroInfo.HeroIndex, heroInfo.Star, heroInfo.Level, heroInfo.Transcended, filteredTypes);
	}

	public static StatData GetEquippedHeroStat(CachedHeroInfo cachedHeroInfo, HeroStatCalculationFlag calculationFlag = HeroStatCalculationFlag.All, StatType[] filteredTypes = null, Dictionary<int, int> resultAddedStatDictionary = null, Dictionary<int, int> resultMultedStatDictionary = null)
	{
		if (cachedHeroInfo == null)
		{
			VespaDebug.LogError("CachedHeroInfo is null : ");
			return new StatData();
		}
		StatData heroStat = GetHeroStat(cachedHeroInfo, filteredTypes);
		if ((calculationFlag & HeroStatCalculationFlag.Equip) != HeroStatCalculationFlag.None)
		{
			foreach (KeyValuePair<int, EquipItemInfo> item in cachedHeroInfo.GetSlotIndexEquipItemPair())
			{
				int key = item.Key;
				EquipItemInfo value = item.Value;
				if (value.IsSoulWeapon())
				{
					StatData soulWeaponStatData = GetSoulWeaponStatData(value);
					heroStat.AddStat(soulWeaponStatData);
				}
				EquipSlotData data = GameRoot.Instance.EquipSlotDataContainer.GetData(key);
				if (data == null || !data.IgnoreStat)
				{
					StatData equipItemStatData = GetEquipItemStatData(value, filteredTypes);
					heroStat.AddStat(equipItemStatData);
				}
			}
		}
		int applyRunePage = cachedHeroInfo.ApplyRunePage;
		List<HeroRunePageInfo> runePageInfos = cachedHeroInfo.GetRunePageInfos(applyRunePage);
		if (runePageInfos != null)
		{
			foreach (HeroRunePageInfo item2 in runePageInfos)
			{
				int itemIndex = item2.ItemIndex;
				_ = item2.EquipItemSlotIndex;
				ItemData data2 = GameRoot.Instance.ItemDataContainer.GetData(itemIndex);
				if (data2 == null)
				{
					VespaDebug.LogError("RuneItemData not found : " + itemIndex);
					continue;
				}
				switch (data2.Type)
				{
				case ItemType.Rune:
				{
					ItemData<RuneItemData> data4 = GameRoot.Instance.ItemDataContainer.GetData<RuneItemData>(itemIndex);
					if (data4 == null)
					{
						VespaDebug.LogError("RuneItemData not found : " + itemIndex);
					}
					else
					{
						heroStat.AddStat(data4.SubData, filteredTypes);
					}
					break;
				}
				case ItemType.PunishmentRune:
				{
					PunishmentRuneOptionInfo punishmentRuneOptionInfo = cachedHeroInfo.GetPunishmentRuneOptionInfo(item2);
					if (punishmentRuneOptionInfo == null)
					{
						VespaDebug.LogError("PunishmentOptionInfo not found  ItemIndex :  " + itemIndex + " EquipSlotIndex : " + item2.EquipItemSlotIndex);
						break;
					}
					PunishmentRuneOptionData data3 = GameRoot.Instance.PunishmentRuneOptionDataContainer.GetData(punishmentRuneOptionInfo.MainOptionIndex);
					if (data3 == null || data3.StatTypes == null)
					{
						VespaDebug.LogError("PunishmentRuneOptionData not found ItemIndex :  " + itemIndex + " OptionIndex : " + punishmentRuneOptionInfo.MainOptionIndex);
						break;
					}
					StatData statData = new StatData();
					StatType[] statTypes = data3.StatTypes;
					foreach (StatType statType in statTypes)
					{
						statData.SetStat(statType, punishmentRuneOptionInfo.MainOptionValue);
					}
					heroStat.AddStat(statData, filteredTypes);
					break;
				}
				}
			}
		}
		IndexableDictionay<int, int> indexableDictionay = new IndexableDictionay<int, int>(16);
		IndexableDictionay<int, int> indexableDictionay2 = new IndexableDictionay<int, int>(16);
		if ((calculationFlag & HeroStatCalculationFlag.EquipOption) != HeroStatCalculationFlag.None)
		{
			using (new SnapshotWritableBlock())
			{
				List<EquipOptionCreatedData> list = new List<EquipOptionCreatedData>();
				cachedHeroInfo.GetEquipOptionDataList(list, null, list);
				foreach (EquipOptionCreatedData item3 in list)
				{
					OperationType optionType = item3.OptionType;
					int optionValue = item3.OptionValue;
					ApplyModifiedStat(optionType, optionValue, filteredTypes, indexableDictionay, indexableDictionay2);
				}
				cachedHeroInfo.GetSetOptionList(out var openedOptionDataList, out var _);
				if (openedOptionDataList != null)
				{
					foreach (EquipOptionData item4 in openedOptionDataList)
					{
						OperationType optionType2 = item4.OptionType;
						int optionValueMin = item4.GetOptionValueMin(0);
						ApplyModifiedStat(optionType2, optionValueMin, filteredTypes, indexableDictionay, indexableDictionay2);
					}
				}
			}
		}
		List<StatData.MultesStatInfo> multedStatList = null;
		heroStat.GetAllMultedExtraStat(out multedStatList);
		if (multedStatList != null)
		{
			for (int j = 0; j < multedStatList.Count; j++)
			{
				StatData.MultesStatInfo multesStatInfo = multedStatList[j];
				StatType type = multesStatInfo.Type;
				int multedValue = (int)multesStatInfo.Value;
				indexableDictionay2.AddOrUpdate((int)type, multedValue, (int k, int v) => v + multedValue);
			}
		}
		HashSet<StatType> hashSet = new HashSet<StatType>();
		for (int num = 0; num < indexableDictionay.Count; num++)
		{
			hashSet.Add((StatType)indexableDictionay.GetKeyByIndex(num));
		}
		for (int num2 = 0; num2 < indexableDictionay2.Count; num2++)
		{
			hashSet.Add((StatType)indexableDictionay2.GetKeyByIndex(num2));
		}
		foreach (StatType item5 in hashSet)
		{
			long stat = heroStat.GetStat(item5);
			int value2 = 0;
			int value3 = 0;
			indexableDictionay2.TryGetValue((int)item5, out value2);
			indexableDictionay.TryGetValue((int)item5, out value3);
			StatAttenuationData data5 = GameRoot.Instance.MultedStatAttenuationDataContainer.GetData(item5);
			if (data5 != null)
			{
				value2 = (int)data5.Attenuate(value2);
			}
			StatAttenuationData data6 = GameRoot.Instance.AddedStatAttenuationDataContainer.GetData(item5);
			if (data6 != null)
			{
				value3 = (int)data6.Attenuate(value3);
			}
			long value4 = StatController.MultAndAddStat(stat, value2, value3);
			heroStat.SetStat(item5, value4);
		}
		if (resultAddedStatDictionary != null)
		{
			foreach (KeyValuePair<int, int> item6 in indexableDictionay)
			{
				resultAddedStatDictionary.Add(item6.Key, item6.Value);
			}
		}
		if (resultMultedStatDictionary != null)
		{
			foreach (KeyValuePair<int, int> item7 in indexableDictionay2)
			{
				resultMultedStatDictionary.Add(item7.Key, item7.Value);
			}
		}
		heroStat.GetStat(StatType.AttackSpeed);
		heroStat.MultStat(StatType.AttackSpeed, GameRoot.Instance.ConstantDataContainer.GetValueInt("BaseAttackSpeedRatio", 850) - 1000);
		return heroStat;
	}

	public static StatData GetEquipStat(EquipItemInfo equipItemInfo, Dictionary<int, int> resultAddedStatDictionary = null, Dictionary<int, int> resultMultedStatDictionary = null, bool applyExtraStat = true)
	{
		StatData statData = new StatData();
		StatType[] filteredTypes = null;
		if (equipItemInfo.IsSoulWeapon())
		{
			StatData soulWeaponStatData = GetSoulWeaponStatData(equipItemInfo);
			statData.AddStat(soulWeaponStatData);
		}
		if (GameRoot.Instance.ItemDataContainer.GetData<EquipItemData>(equipItemInfo.ItemIndex) == null)
		{
			return statData;
		}
		StatData equipItemStatData = GetEquipItemStatData(equipItemInfo, filteredTypes);
		statData.AddStat(equipItemStatData);
		GetExtraStat(equipItemInfo, filteredTypes, statData, out var addedStatDictionary, out var multedStatDictionary);
		if (applyExtraStat)
		{
			ApplyStat(statData, addedStatDictionary, multedStatDictionary);
		}
		SetResultStatDictionary(resultAddedStatDictionary, resultMultedStatDictionary, addedStatDictionary, multedStatDictionary);
		return statData;
	}

	private static void SetResultStatDictionary(Dictionary<int, int> resultAddedStatDictionary, Dictionary<int, int> resultMultedStatDictionary, IndexableDictionay<int, int> addedStatDictionary, IndexableDictionay<int, int> multedStatDictionary)
	{
		if (resultAddedStatDictionary != null)
		{
			foreach (KeyValuePair<int, int> item in addedStatDictionary)
			{
				resultAddedStatDictionary.Add(item.Key, item.Value);
			}
		}
		if (resultMultedStatDictionary == null)
		{
			return;
		}
		foreach (KeyValuePair<int, int> item2 in multedStatDictionary)
		{
			resultMultedStatDictionary.Add(item2.Key, item2.Value);
		}
	}

	private static void GetExtraStat(EquipItemInfo equipItemInfo, StatType[] filteredTypes, StatData statData, out IndexableDictionay<int, int> addedStatDictionary, out IndexableDictionay<int, int> multedStatDictionary)
	{
		addedStatDictionary = new IndexableDictionay<int, int>(16);
		multedStatDictionary = new IndexableDictionay<int, int>(16);
		List<EquipOptionCreatedData> list = new List<EquipOptionCreatedData>();
		equipItemInfo.GetEquipOptionDataList(list, null, list);
		foreach (EquipOptionCreatedData item in list)
		{
			OperationType optionType = item.OptionType;
			int optionValue = item.OptionValue;
			ApplyModifiedStat(optionType, optionValue, filteredTypes, addedStatDictionary, multedStatDictionary);
		}
		List<StatData.MultesStatInfo> multedStatList = null;
		statData.GetAllMultedExtraStat(out multedStatList);
		if (multedStatList == null)
		{
			return;
		}
		for (int i = 0; i < multedStatList.Count; i++)
		{
			StatData.MultesStatInfo multesStatInfo = multedStatList[i];
			StatType type = multesStatInfo.Type;
			int multedValue = (int)multesStatInfo.Value;
			multedStatDictionary.AddOrUpdate((int)type, multedValue, (int k, int v) => v + multedValue);
		}
	}

	private static void ApplyStat(StatData statData, IndexableDictionay<int, int> addedStatDictionary, IndexableDictionay<int, int> multedStatDictionary)
	{
		HashSet<StatType> hashSet = new HashSet<StatType>();
		for (int i = 0; i < addedStatDictionary.Count; i++)
		{
			hashSet.Add((StatType)addedStatDictionary.GetKeyByIndex(i));
		}
		for (int j = 0; j < multedStatDictionary.Count; j++)
		{
			hashSet.Add((StatType)multedStatDictionary.GetKeyByIndex(j));
		}
		foreach (StatType item in hashSet)
		{
			long stat = statData.GetStat(item);
			int value = 0;
			int value2 = 0;
			multedStatDictionary.TryGetValue((int)item, out value);
			addedStatDictionary.TryGetValue((int)item, out value2);
			StatAttenuationData data = GameRoot.Instance.MultedStatAttenuationDataContainer.GetData(item);
			if (data != null)
			{
				value = (int)data.Attenuate(value);
			}
			StatAttenuationData data2 = GameRoot.Instance.AddedStatAttenuationDataContainer.GetData(item);
			if (data2 != null)
			{
				value2 = (int)data2.Attenuate(value2);
			}
			long value3 = StatController.MultAndAddStat(stat, value, value2);
			statData.SetStat(item, value3);
		}
	}

	private static void ApplyModifiedStat(OperationType optionType, int optionValue, StatType[] filteredTypes, IndexableDictionay<int, int> addedStatDictionary, IndexableDictionay<int, int> multedStatDictionary)
	{
		if (!(GameRoot.Instance.OperationRuntimeFactory.CreateForStatCalculation(optionType, optionValue) is IStatModifier statModifier))
		{
			return;
		}
		StatType[] statTypes = statModifier.GetStatTypes();
		int addedStat = statModifier.GetAddedStat();
		int multedStat = statModifier.GetMultedStat();
		StatType[] array = statTypes;
		foreach (StatType statType in array)
		{
			if (filteredTypes == null || filteredTypes.Contains(statType))
			{
				addedStatDictionary.AddOrUpdate((int)statType, addedStat, (int k, int v) => v + addedStat);
				multedStatDictionary.AddOrUpdate((int)statType, multedStat, (int k, int v) => v + multedStat);
			}
		}
	}

	public static StatData GetEquipStat(CachedHeroInfo heroInfo)
	{
		StatData statData = new StatData();
		EquipItemInfo[] equipItemInfos = heroInfo.GetEquipItemInfos();
		foreach (EquipItemInfo equipItemInfo in equipItemInfos)
		{
			statData.AddStat(GetEquipItemStatData(equipItemInfo));
		}
		return statData;
	}

	public static StatData GetEquipItemStatData(EquipItemInfo equipItemInfo, StatType[] filteredTypes = null, int level = 0)
	{
		if (equipItemInfo == null)
		{
			return new StatData();
		}
		int itemIndex = equipItemInfo.ItemIndex;
		int star = equipItemInfo.Star;
		if (level == 0)
		{
			level = equipItemInfo.Level;
		}
		return GetEquipItemStatData(itemIndex, star, level, filteredTypes);
	}

	public static StatData GetEquipItemStatData(int itemIndex, int star, int level, StatType[] filteredTypes = null)
	{
		ItemData<EquipItemData> data = GameRoot.Instance.ItemDataContainer.GetData<EquipItemData>(itemIndex);
		if (data == null)
		{
			return new StatData();
		}
		EquipItemStarData data2 = GameRoot.Instance.EquipItemStarDataContainer.GetData(star);
		if (data2 == null)
		{
			VespaDebug.LogError("EquipStarData not found. itemIndex=" + itemIndex + " star=" + star);
			return new StatData();
		}
		int detailIndex = data.SubData.DetailIndex;
		EquipUpgradeStatData data3 = GameRoot.Instance.EquipUpgradeStatDataContainer.GetData(star, detailIndex, level);
		if (data3 == null)
		{
			VespaDebug.LogError("UpgradeStat not found. itemIndex=" + itemIndex + " star=" + star + " detailIndex=" + detailIndex + " level=" + level);
			return new StatData();
		}
		StatData statData = new StatData();
		StatType[] array = ((filteredTypes != null && filteredTypes.Length != 0) ? filteredTypes : StatData.StatTypes);
		foreach (StatType statType in array)
		{
			long stat = data.SubData.GetStat(statType);
			long equipFinalStat = GetEquipFinalStat(statType, stat, data2, data3);
			statData.SetStat(statType, equipFinalStat);
		}
		return statData;
	}

	public static long GetEquipFinalStat(StatType statType, long equipBaseStat, EquipItemStarData starData, EquipUpgradeStatData upgradeStatData)
	{
		int stat = upgradeStatData.GetStat(statType);
		int num = ((equipBaseStat != 0L) ? upgradeStatData.GetAddStat(statType) : 0);
		return (equipBaseStat * stat + num * 1000) * starData.StatFactor / 1000 / 1000;
	}

	public static long GetCreatureWavePositionScore(CachedHeroInfo heroInfo)
	{
		if (heroInfo == null)
		{
			return 0L;
		}
		int heroIndex = heroInfo.HeroIndex;
		StatData equippedHeroStat = GetEquippedHeroStat(heroInfo, HeroStatCalculationFlag.All, new StatType[4]
		{
			StatType.PhysicalDefense,
			StatType.AddPhysicalDefenseR,
			StatType.MagicalDefense,
			StatType.AddMagicalDefenseR
		});
		return GetCreatureWavePositionScore(heroIndex, equippedHeroStat);
	}

	public static long GetCreatureWavePositionScore(int creatureIndex, StatData statData)
	{
		CreatureData data = GameRoot.Instance.CreatureDataContainer.GetData(creatureIndex);
		int positionWeight = data?.PositionWeight ?? 0;
		CreaturePositionType positionType = data?.PositionType ?? CreaturePositionType.Forward;
		long maxStat = statData.GetMaxStat(StatType.PhysicalDefense, StatType.MagicalDefense);
		return GetCreatureWavePositionScore(positionType, positionWeight, maxStat, creatureIndex);
	}

	public static long GetCreatureWavePositionScore(CreaturePositionType positionType, int positionWeight, long defence, int heroIndex)
	{
		return (long)positionType * 10000L * 10000 * 10000 + (long)positionWeight * 10000L * 10000 + defence + heroIndex;
	}

	public static StatData GetSoulWeaponStatData(EquipItemInfo equipItemInfo)
	{
		if (equipItemInfo == null)
		{
			return new StatData();
		}
		if (!equipItemInfo.IsSoulWeapon())
		{
			return new StatData();
		}
		return GetSoulWeaponStatData(equipItemInfo.SoulWeaponInfo);
	}

	public static StatData GetSoulWeaponStatData(SoulWeaponInfo soulWeaponInfo)
	{
		StatData statData = new StatData();
		if (soulWeaponInfo == null)
		{
			return statData;
		}
		SoulWeaponOptionData soulWeaponOptionData = soulWeaponInfo.GetSoulWeaponOptionData();
		if (soulWeaponOptionData == null)
		{
			return statData;
		}
		SoulWeaponReinforceData soulWeaponReinforceData = soulWeaponInfo.GetSoulWeaponReinforceData();
		if (soulWeaponReinforceData == null)
		{
			return statData;
		}
		SoulWeaponUpgradeData soulWeaponUpgradeData = soulWeaponInfo.GetSoulWeaponUpgradeData();
		if (soulWeaponUpgradeData == null)
		{
			return statData;
		}
		int statCount = soulWeaponInfo.GetStatCount();
		ESoulWeaponStatType eSoulWeaponStatType = ESoulWeaponStatType.End;
		for (int i = 0; i < statCount; i++)
		{
			StatType[] optionStatTypes = soulWeaponOptionData.GetOptionStatTypes(i);
			if (optionStatTypes != null && optionStatTypes.Length != 0)
			{
				eSoulWeaponStatType = (ESoulWeaponStatType)i;
				int optionRatio = soulWeaponInfo.GetOptionRatio(eSoulWeaponStatType);
				int optionRatioPerValue = soulWeaponOptionData.GetOptionRatioPerValue(eSoulWeaponStatType);
				int optionBonusRatio = soulWeaponInfo.GetOptionBonusRatio(eSoulWeaponStatType);
				int statRate = soulWeaponReinforceData.StatRate;
				int upgradeRate = soulWeaponUpgradeData.UpgradeRate;
				foreach (StatType statType in optionStatTypes)
				{
					int soulWeaponFinalStat = GetSoulWeaponFinalStat(optionRatio, optionRatioPerValue, optionBonusRatio, statRate, upgradeRate);
					statData.SetStat(statType, soulWeaponFinalStat);
				}
			}
		}
		return statData;
	}

	public static int GetSoulWeaponBaseStat(int ratio, int maxValue, int reinforceRate, int upgradeRate)
	{
		long num = (long)(1000 + reinforceRate) * (long)(1000 + upgradeRate) / 1000;
		return (int)(ratio * maxValue * num / 1000);
	}

	public static int GetSoulWeaponBonusStat(int baseStat, int bonusRate)
	{
		if (bonusRate == 0)
		{
			return 0;
		}
		return (int)((long)baseStat * (long)bonusRate / 1000);
	}

	private static int GetSoulWeaponFinalStat(int ratio, int perRatio, int bonusRate, int reinforceRate, int gradeRate)
	{
		int soulWeaponBaseStat = GetSoulWeaponBaseStat(ratio, perRatio, reinforceRate, gradeRate);
		int soulWeaponBonusStat = GetSoulWeaponBonusStat(soulWeaponBaseStat, bonusRate);
		return soulWeaponBaseStat + soulWeaponBonusStat;
	}

	public static int GetEquipOptionAddedSkillLevel(CachedHeroInfo heroInfo, int skillSlotIndex)
	{
		if (heroInfo == null)
		{
			return 0;
		}
		StatData equippedHeroStat = GetEquippedHeroStat(heroInfo, HeroStatCalculationFlag.All, new StatType[5]
		{
			StatType.SkillLevelAllSlot,
			StatType.SkillLevelSlot1,
			StatType.SkillLevelSlot2,
			StatType.SkillLevelSlot3,
			StatType.SkillLevelSlot4
		});
		if (equippedHeroStat == null)
		{
			return 0;
		}
		return GetEquipOptionAddedSkillLevel(equippedHeroStat, skillSlotIndex);
	}

	public static int GetEquipOptionAddedSkillLevel(StatData statData, int skillSlotIndex)
	{
		if (skillSlotIndex > 4)
		{
			return 0;
		}
		long num = 0L;
		switch (skillSlotIndex)
		{
		case 1:
			num = statData.GetStat(StatType.SkillLevelSlot1);
			break;
		case 2:
			num = statData.GetStat(StatType.SkillLevelSlot2);
			break;
		case 3:
			num = statData.GetStat(StatType.SkillLevelSlot3);
			break;
		case 4:
			num = statData.GetStat(StatType.SkillLevelSlot4);
			break;
		}
		num += statData.GetStat(StatType.SkillLevelAllSlot);
		return (int)num;
	}

	public static void ApplyExtraStat(StatData heroStatData, IList<IConditionalExtraStatData> extraStatDatas)
	{
		if (extraStatDatas == null || extraStatDatas.Count == 0)
		{
			return;
		}
		IndexableDictionay<int, int> indexableDictionay = new IndexableDictionay<int, int>(16);
		IndexableDictionay<int, int> indexableDictionay2 = new IndexableDictionay<int, int>(16);
		for (int i = 0; i < extraStatDatas.Count; i++)
		{
			IConditionalExtraStatData conditionalExtraStatData = extraStatDatas[i];
			if (conditionalExtraStatData != null && conditionalExtraStatData.OptionType != OperationType.UseSkill)
			{
				OperationType optionType = conditionalExtraStatData.OptionType;
				int optionValue = conditionalExtraStatData.OptionValue;
				ApplyModifiedStat(optionType, optionValue, null, indexableDictionay, indexableDictionay2);
			}
		}
		if (indexableDictionay.Count != 0 || indexableDictionay2.Count != 0)
		{
			ApplyStat(heroStatData, indexableDictionay, indexableDictionay2);
		}
	}
}
