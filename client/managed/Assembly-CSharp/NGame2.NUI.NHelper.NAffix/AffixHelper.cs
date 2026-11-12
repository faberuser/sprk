using System;
using System.Collections.Generic;
using System.Linq;
using NGame2.NUI.NWindow;
using NGame2.NUtil;
using NShared;
using NVespa;
using NVespa.NGlobal;

namespace NGame2.NUI.NHelper.NAffix;

public static class AffixHelper
{
	public class LocalStroageAffixSetting
	{
		private readonly LocalStorageBool _affixSave;

		public LocalStroageAffixSetting()
		{
			_affixSave = new LocalStorageBool(i_default: true, "AlwaysSaveAffixSetting");
			_affixSave.Init();
		}

		public void Set(bool isAlwaysSave)
		{
			_affixSave.Value = isAlwaysSave;
		}

		public bool Get()
		{
			return _affixSave.Value;
		}
	}

	public class LocalStorageAffixIndices
	{
		private readonly LocalAccountIntArray _affixArray;

		public LocalStorageAffixIndices(int chapterIndex, int dungeonIndex, int difficulty)
		{
			string i_keyPrefix = CreateKey(chapterIndex, dungeonIndex, difficulty);
			_affixArray = new LocalAccountIntArray(0, i_keyPrefix);
			_affixArray.Init();
		}

		private string CreateKey(int chapterIndex, int dungeonIndex, int diffculty)
		{
			return $"{chapterIndex}_{dungeonIndex}_{diffculty}_AffixArray_";
		}

		public int[] GetAffixIndices(int toalCount)
		{
			List<int> list = new List<int>();
			for (int i = 0; i < toalCount; i++)
			{
				int num = _affixArray[i];
				if (num != 0)
				{
					list.Add(num);
				}
			}
			return list.ToArray();
		}

		public void Save(int[] affixIndices, int totalCount)
		{
			for (int i = 0; i < totalCount; i++)
			{
				_affixArray[i] = 0;
			}
			if (!__Extension_Method_ICollection__.IsNullOrEmpty(affixIndices))
			{
				for (int j = 0; j < affixIndices.Length; j++)
				{
					_affixArray[j] = affixIndices[j];
				}
			}
		}
	}

	public static void SaveAffixIndices(int chapterIndex, int dungeonIndex, int difficulty, int[] affixIndices, int totalCount)
	{
		new LocalStorageAffixIndices(chapterIndex, dungeonIndex, difficulty).Save(affixIndices, totalCount);
	}

	public static int[] LoadLocalStorageAffixIndices(int chapterIndex, int dungeonIndex, int difficulty, int totalCount)
	{
		return new LocalStorageAffixIndices(chapterIndex, dungeonIndex, difficulty).GetAffixIndices(totalCount);
	}

	public static void SetAlwaysSave(bool isAlwaysSave)
	{
		new LocalStroageAffixSetting().Set(isAlwaysSave);
	}

	public static bool GetAlwaysSave()
	{
		return new LocalStroageAffixSetting().Get();
	}

	public static int[] GetPunishmentAffixIndices(PunishmentRaidData punishmentRaidData)
	{
		if (punishmentRaidData == null)
		{
			return null;
		}
		List<int> list = new List<int>();
		int[] triggerIndices = punishmentRaidData.TriggerIndices;
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(triggerIndices); i++)
		{
			PunishmentRaidTriggerData data = GameRoot.Instance.PunishmentRaidTriggerDataContainer.GetData(triggerIndices[i]);
			if (SingletonAuto<ConditionChecker>.instance.Check(data.Condition))
			{
				list.AddRange(data.AffixIndices);
			}
		}
		int raidIndex = punishmentRaidData.RaidIndex;
		int raidLevel = punishmentRaidData.RaidLevel;
		RaidData data2 = GameRoot.Instance.RaidDataContainer.GetData(raidIndex, raidLevel);
		if (data2 == null)
		{
			if (__Extension_Method_ICollection__.COUNT(list) <= 0)
			{
				return null;
			}
			return list.ToArray();
		}
		int chapterIndex = data2.ChapterIndex;
		int dungeonIndex = data2.DungeonIndex;
		DungeonDifficultyType difficultyType = DungeonDifficultyType.Easy;
		int[] autoApplyAffixIndices = GetAutoApplyAffixIndices(chapterIndex, dungeonIndex, difficultyType);
		if (!__Extension_Method_ICollection__.IsNullOrEmpty(autoApplyAffixIndices))
		{
			list.AddRange(autoApplyAffixIndices);
		}
		if (__Extension_Method_ICollection__.COUNT(list) <= 0)
		{
			return null;
		}
		return list.ToArray();
	}

	public static int[] GetAutoApplyAffixIndices(int chatperIndex, int dungeonIndex, DungeonDifficultyType difficultyType)
	{
		CampaignDungeonData data = GameRoot.Instance.CampaignDungeonDataContainer.GetData(chatperIndex, dungeonIndex);
		if (data == null)
		{
			return null;
		}
		int affixGroupIndex = data.GetAffixGroupIndex(difficultyType);
		IList<AffixData> affixDataListByGroupIndex = GameRoot.Instance.AffixDataContainer.GetAffixDataListByGroupIndex(affixGroupIndex);
		if (__Extension_Method_ICollection__.IsNullOrEmpty(affixDataListByGroupIndex))
		{
			return null;
		}
		List<int> list = new List<int>();
		for (int i = 0; i < affixDataListByGroupIndex.Count; i++)
		{
			AffixData affixData = affixDataListByGroupIndex[i];
			if (affixData != null && IsAutoApplyBattle(affixData))
			{
				list.Add(affixData.Index);
			}
		}
		return list.ToArray();
	}

	public static bool IsAutoApplyBattle(AffixData affixData)
	{
		if (__Extension_Method_ICollection__.IsNullOrEmpty(affixData.AutoApplyConditionValues))
		{
			return false;
		}
		string[] autoApplyConditionValues = affixData.AutoApplyConditionValues;
		if (!SingletonAuto<ConditionChecker>.instance.Check(autoApplyConditionValues))
		{
			return false;
		}
		return true;
	}

	public static bool IsAutoApplyBattle(AffixData affixData, int infinityLevel)
	{
		if (__Extension_Method_ICollection__.IsNullOrEmpty(affixData.AutoApplyConditionValues))
		{
			return false;
		}
		string[] autoApplyConditionValues = affixData.AutoApplyConditionValues;
		if (__Extension_Method_ICollection__.IsNullOrEmpty(autoApplyConditionValues))
		{
			return false;
		}
		int num = autoApplyConditionValues.Length + 1;
		string[] array = new string[num];
		Array.Copy(autoApplyConditionValues, array, autoApplyConditionValues.Length);
		array[num - 1] = infinityLevel.ToString();
		if (!SingletonAuto<ConditionChecker>.instance.Check(array))
		{
			return false;
		}
		return true;
	}

	public static string GetAffixDesc(AffixData affixData, int infinityLevel = 0)
	{
		if (affixData == null)
		{
			return string.Empty;
		}
		string text = "";
		switch (affixData.AffixType)
		{
		case EAffixType.Stat:
			text = GetAffisStatDesc(affixData);
			break;
		case EAffixType.Skill:
			text = GetAffisSkillDesc(affixData);
			break;
		case EAffixType.InfinityStat:
			text = GetInfinityAffisStatDesc(affixData, infinityLevel);
			break;
		}
		string descKey = affixData.DescKey;
		if (String_Extension_Method.IsNullOrEmpty(descKey))
		{
			return text;
		}
		return LocalizationManager.GetStringDirect(descKey, text);
	}

	private static string GetAffisStatDesc(AffixData affixData)
	{
		string arg = LocalizationManager.GetString(affixData.OptionType);
		string statDisplayValue = StatData.GetStatDisplayValue(affixData.OptionType, affixData.OptionValue, showSign: true, isCommaString: true);
		return $"{arg} {statDisplayValue}";
	}

	private static string GetAffisSkillDesc(AffixData affixData)
	{
		SkillData data = GameRoot.Instance.SkillDataContainer.GetData(affixData.OptionValue);
		if (data == null)
		{
			return "Not Exist Skill Index : " + affixData.OptionValue;
		}
		return data.Description;
	}

	private static string GetInfinityAffisStatDesc(AffixData affixData, int infinityLevel)
	{
		int infinityIndex = affixData.InfinityIndex;
		int infinityStep = affixData.GetInfinityStep(infinityLevel);
		int sumValue = GameRoot.Instance.InfinityDataContainer.GetSumValue(infinityIndex, infinityStep);
		string arg = LocalizationManager.GetString(affixData.OptionType);
		string statDisplayValue = StatData.GetStatDisplayValue(affixData.OptionType, sumValue, showSign: true, isCommaString: true);
		return $"{arg} {statDisplayValue}";
	}

	public static int GetTotalAffixPoint(CampaignDungeonData dungeonData, DungeonDifficultyType difficultyType, int[] affixIndices)
	{
		int clearPoint = GetClearPoint(dungeonData, difficultyType);
		int affixPoint = GetAffixPoint(affixIndices);
		float affixMultiple = GetAffixMultiple(dungeonData, difficultyType);
		return (int)((float)(clearPoint + affixPoint) * affixMultiple);
	}

	private static int GetClearPoint(CampaignDungeonData dungeonData, DungeonDifficultyType difficultyType)
	{
		return dungeonData?.GetClearPoint(difficultyType) ?? 0;
	}

	private static int GetAffixPoint(int[] affixIndices)
	{
		if (affixIndices == null)
		{
			return 0;
		}
		int num = 0;
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(affixIndices); i++)
		{
			int key = affixIndices[i];
			AffixData data = GameRoot.Instance.AffixDataContainer.GetData(key);
			if (data != null)
			{
				num += data.Point;
			}
		}
		return num;
	}

	private static float GetAffixMultiple(CampaignDungeonData dungeonData, DungeonDifficultyType difficultyType)
	{
		if (dungeonData == null)
		{
			return 0f;
		}
		return (float)dungeonData.GetAffixMultiple(difficultyType) / 1000f;
	}

	public static bool IsEqualAffixIndices(int[] lhs, int[] rhs)
	{
		int num = __Extension_Method_ICollection__.COUNT(lhs);
		int num2 = __Extension_Method_ICollection__.COUNT(rhs);
		if (num != num2)
		{
			return false;
		}
		if (num == 0)
		{
			return true;
		}
		Array.Sort(lhs, Compare);
		Array.Sort(rhs, Compare);
		return lhs.SequenceEqual(rhs);
	}

	private static int Compare(int x, int y)
	{
		return x.CompareTo(y);
	}

	public static bool CheckBanRule(List<BanRuleData> banRuleList, List<int> heroIndexList)
	{
		if (__Extension_Method_ICollection__.IsNullOrEmpty(banRuleList) || __Extension_Method_ICollection__.IsNullOrEmpty(heroIndexList))
		{
			return false;
		}
		List<RaidBanRuleData> raidBanRuleDataList = GetRaidBanRuleDataList(banRuleList);
		if (__Extension_Method_ICollection__.IsNullOrEmpty(raidBanRuleDataList))
		{
			return false;
		}
		List<CreatureData> creatureDataList = GetCreatureDataList(heroIndexList);
		if (__Extension_Method_ICollection__.IsNullOrEmpty(creatureDataList))
		{
			return false;
		}
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(raidBanRuleDataList); i++)
		{
			if (CheckRaidBanRuleData(raidBanRuleDataList[i], creatureDataList))
			{
				return true;
			}
		}
		return false;
	}

	public static string GetBanMessage(List<BanRuleData> banRuleList, int heroIndex, List<int> heroIndexList)
	{
		if (__Extension_Method_ICollection__.IsNullOrEmpty(banRuleList))
		{
			return string.Empty;
		}
		List<RaidBanRuleData> raidBanRuleDataList = GetRaidBanRuleDataList(banRuleList);
		if (__Extension_Method_ICollection__.IsNullOrEmpty(raidBanRuleDataList))
		{
			return string.Empty;
		}
		CreatureData data = GameRoot.Instance.CreatureDataContainer.GetData(heroIndex);
		if (data == null)
		{
			return string.Empty;
		}
		List<CreatureData> creatureDataList = GetCreatureDataList(heroIndexList);
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(raidBanRuleDataList); i++)
		{
			string banMessage = GetBanMessage(raidBanRuleDataList[i], data, creatureDataList);
			if (!String_Extension_Method.IsNullOrEmpty(banMessage))
			{
				return banMessage;
			}
		}
		return string.Empty;
	}

	private static string GetBanMessage(RaidBanRuleData raidBanRuleData, CreatureData creatureData, List<CreatureData> creatureDataList)
	{
		if (raidBanRuleData == null || creatureData == null)
		{
			return string.Empty;
		}
		RaidBanType banType = raidBanRuleData.BanType;
		string banValue = raidBanRuleData.BanValue;
		switch (banType)
		{
		case RaidBanType.SkillAttrType:
		{
			SkillAttrType type = SkillAttrType.None;
			if (!BanRuleData.GetSkillAttrType(banValue, out type))
			{
				return string.Empty;
			}
			if (IsBanSkillAttrType(creatureData, type))
			{
				return "SkillAttrType" + type;
			}
			break;
		}
		case RaidBanType.CodeName:
		{
			string[] codeNames = BanRuleData.GetCodeNames(banValue);
			if (IsBanCodeName(creatureData, codeNames))
			{
				return "CreatureCodeBan";
			}
			break;
		}
		case RaidBanType.CreatureTagType:
		{
			CreatureTagType type2 = CreatureTagType.None;
			if (!BanRuleData.GetCreatureTagType(banValue, out type2))
			{
				return string.Empty;
			}
			if (IsBanCreatureTagType(creatureData, type2))
			{
				return "CreatureTagType Ban";
			}
			break;
		}
		case RaidBanType.ClassEnterPermission:
		{
			Tuple<List<CreatureTagType>, int> classEnterBanRuleData = RaidHelper.GetClassEnterBanRuleData(raidBanRuleData);
			if (__Extension_Method_ICollection__.IsNullOrEmpty(creatureDataList))
			{
				creatureDataList = new List<CreatureData>();
			}
			creatureDataList.Add(creatureData);
			if (IsBanClassEnterPermission(creatureDataList, classEnterBanRuleData))
			{
				return "Class EnterPermission Ban";
			}
			break;
		}
		}
		return string.Empty;
	}

	private static bool CheckRaidBanRuleData(RaidBanRuleData raidBanRuleData, List<CreatureData> creatureDataList)
	{
		if (raidBanRuleData == null || __Extension_Method_ICollection__.IsNullOrEmpty(creatureDataList))
		{
			return false;
		}
		RaidBanType banType = raidBanRuleData.BanType;
		string banValue = raidBanRuleData.BanValue;
		switch (banType)
		{
		case RaidBanType.SkillAttrType:
		{
			SkillAttrType type2 = SkillAttrType.None;
			if (!BanRuleData.GetSkillAttrType(banValue, out type2))
			{
				return false;
			}
			return IsBanSkillAttrType(creatureDataList, type2);
		}
		case RaidBanType.CodeName:
		{
			string[] codeNames = BanRuleData.GetCodeNames(banValue);
			return IsBanCodeName(creatureDataList, codeNames);
		}
		case RaidBanType.CreatureTagType:
		{
			CreatureTagType type = CreatureTagType.None;
			if (!BanRuleData.GetCreatureTagType(banValue, out type))
			{
				return false;
			}
			return IsBanCreatureTagType(creatureDataList, type);
		}
		case RaidBanType.ClassEnterPermission:
		{
			Tuple<List<CreatureTagType>, int> classEnterBanRuleData = RaidHelper.GetClassEnterBanRuleData(raidBanRuleData);
			return IsBanClassEnterPermission(creatureDataList, classEnterBanRuleData);
		}
		default:
			return false;
		}
	}

	public static bool IsBanSkillAttrType(List<CreatureData> creatureDataList, SkillAttrType skillAttrType)
	{
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(creatureDataList); i++)
		{
			if (IsBanSkillAttrType(creatureDataList[i], skillAttrType))
			{
				return true;
			}
		}
		return false;
	}

	public static bool IsBanSkillAttrType(CreatureData creatureData, SkillAttrType skillAttrType)
	{
		if (creatureData == null)
		{
			return false;
		}
		return creatureData.HeroAttrType == skillAttrType;
	}

	public static bool IsBanCreatureTagType(List<CreatureData> creatureDataList, CreatureTagType creatureTagType)
	{
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(creatureDataList); i++)
		{
			if (IsBanCreatureTagType(creatureDataList[i], creatureTagType))
			{
				return true;
			}
		}
		return false;
	}

	public static bool IsBanCreatureTagType(CreatureData creatureData, CreatureTagType creatureTagType)
	{
		if (creatureData == null)
		{
			return false;
		}
		return creatureData.TagType == creatureTagType;
	}

	public static bool IsBanCodeName(List<CreatureData> creatureDataList, string[] banCodeNames)
	{
		if (__Extension_Method_ICollection__.IsNullOrEmpty(creatureDataList) || __Extension_Method_ICollection__.IsNullOrEmpty(banCodeNames))
		{
			return false;
		}
		HashSet<string> hashSet = new HashSet<string>();
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(creatureDataList); i++)
		{
			CreatureData creatureData = creatureDataList[i];
			if (creatureData != null)
			{
				string codeName = creatureData.CodeName;
				if (!hashSet.Contains(codeName))
				{
					hashSet.Add(codeName);
				}
			}
		}
		for (int j = 0; j < __Extension_Method_ICollection__.COUNT(banCodeNames); j++)
		{
			string item = banCodeNames[j];
			if (hashSet.Contains(item))
			{
				return true;
			}
		}
		return false;
	}

	public static bool IsBanCodeName(CreatureData creatureData, string[] banCreatureCodeArray)
	{
		if (creatureData == null || __Extension_Method_ICollection__.IsNullOrEmpty(banCreatureCodeArray))
		{
			return false;
		}
		string codeName = creatureData.CodeName;
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(banCreatureCodeArray); i++)
		{
			if (banCreatureCodeArray[i] == codeName)
			{
				return true;
			}
		}
		return false;
	}

	public static bool IsBanClassEnterPermission(List<CreatureData> creatureDataList, Tuple<List<CreatureTagType>, int> ban)
	{
		if (ban == null || __Extension_Method_ICollection__.IsNullOrEmpty(ban.Item1) || __Extension_Method_ICollection__.IsNullOrEmpty(creatureDataList))
		{
			return false;
		}
		Dictionary<CreatureTagType, int> dictionary = new Dictionary<CreatureTagType, int>();
		List<CreatureTagType> item = ban.Item1;
		int item2 = ban.Item2;
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(creatureDataList); i++)
		{
			CreatureData creatureData = creatureDataList[i];
			if (creatureData == null)
			{
				continue;
			}
			CreatureTagType tagType = creatureData.TagType;
			if (item.Contains(tagType))
			{
				int value = 0;
				dictionary.TryGetValue(tagType, out value);
				value = (dictionary[tagType] = value + 1);
				if (value > item2)
				{
					return true;
				}
			}
		}
		return false;
	}

	private static List<CreatureData> GetCreatureDataList(List<int> heroIndices)
	{
		List<CreatureData> list = null;
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(heroIndices); i++)
		{
			int key = heroIndices[i];
			CreatureData data = GameRoot.Instance.CreatureDataContainer.GetData(key);
			if (data != null)
			{
				if (list == null)
				{
					list = new List<CreatureData>();
				}
				list.Add(data);
			}
		}
		return list;
	}

	public static List<RaidBanRuleData> GetRaidBanRuleDataList(List<BanRuleData> banRuleList)
	{
		List<RaidBanRuleData> list = null;
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(banRuleList); i++)
		{
			BanRuleData banRuleData = banRuleList[i];
			if (banRuleData == null)
			{
				continue;
			}
			RaidBanRuleData[] banRuleDataAry = banRuleData.GetBanRuleDataAry();
			for (int j = 0; j < __Extension_Method_ICollection__.COUNT(banRuleDataAry); j++)
			{
				RaidBanRuleData raidBanRuleData = banRuleDataAry[j];
				if (raidBanRuleData == null || String_Extension_Method.IsNullOrEmpty(raidBanRuleData.BanValue))
				{
					continue;
				}
				RaidBanType banType = raidBanRuleData.BanType;
				if (banType != RaidBanType.None && banType != RaidBanType.AttributePermission)
				{
					if (list == null)
					{
						list = new List<RaidBanRuleData>();
					}
					list.Add(raidBanRuleData);
				}
			}
		}
		return list;
	}

	private static int Compare(AffixData lhs, AffixData rhs)
	{
		int num = lhs?.Index ?? 0;
		int value = rhs?.Index ?? 0;
		return num.CompareTo(value);
	}

	public static List<AffixData> GetBanRuleAffixList(int[] affixIndices)
	{
		List<AffixData> list = null;
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(affixIndices); i++)
		{
			int key = affixIndices[i];
			AffixData data = GameRoot.Instance.AffixDataContainer.GetData(key);
			if (data != null && data.AffixType == EAffixType.BanRule)
			{
				if (list == null)
				{
					list = new List<AffixData>();
				}
				list.Add(data);
			}
		}
		return list;
	}

	public static int[] GetBanRuleIndices(int[] affixIndices)
	{
		List<int> list = null;
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(affixIndices); i++)
		{
			int key = affixIndices[i];
			AffixData data = GameRoot.Instance.AffixDataContainer.GetData(key);
			if (data != null && data.AffixType == EAffixType.BanRule)
			{
				if (list == null)
				{
					list = new List<int>();
				}
				int banRuleIndex = data.BanRuleIndex;
				list.Add(banRuleIndex);
			}
		}
		if (!__Extension_Method_ICollection__.IsNullOrEmpty(list))
		{
			return list.ToArray();
		}
		return null;
	}

	public static List<BanRuleData> GetBanRuleDataList(int[] affixIndices)
	{
		List<BanRuleData> list = null;
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(affixIndices); i++)
		{
			int key = affixIndices[i];
			AffixData data = GameRoot.Instance.AffixDataContainer.GetData(key);
			if (data == null || data.AffixType != EAffixType.BanRule)
			{
				continue;
			}
			int banRuleIndex = data.BanRuleIndex;
			BanRuleData data2 = GameRoot.Instance.BanRuleDataContainer.GetData(banRuleIndex);
			if (data2 != null)
			{
				if (list == null)
				{
					list = new List<BanRuleData>();
				}
				list.Add(data2);
			}
		}
		return list;
	}
}
