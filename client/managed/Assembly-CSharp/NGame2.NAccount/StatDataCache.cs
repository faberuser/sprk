using System.Collections.Generic;
using System.Diagnostics;
using NGame2.NAccount.NStatDataCache;
using NShared;
using NVespa;
using NVespa.NSingleton;

namespace NGame2.NAccount;

public class StatDataCache : SceneAutoBehaviour<StatDataCache>
{
	public class CachedStatData
	{
		public StatData StatData;

		public Dictionary<int, int> AddedStatDictionary;

		public Dictionary<int, int> MultedStatDictionary;
	}

	public static LocalEditorBool SkipVerifyStatData = new LocalEditorBool(i_default: false, typeof(StatDataCache).FullName + "_skipVerifyStatData");

	private Dictionary<Tuple<int, int>, CachedHeroInfo> _referenceHeroDic = new Dictionary<Tuple<int, int>, CachedHeroInfo>();

	private Dictionary<HeroKey, StatData> _heroDic = new Dictionary<HeroKey, StatData>(new HeroKeyComparer());

	private Dictionary<EquipItemKey, StatData> _equipItemDic = new Dictionary<EquipItemKey, StatData>(new EquipItemKeyComparer());

	private Dictionary<HeroEquipStatKey, StatData> _heroEquipStatDic = new Dictionary<HeroEquipStatKey, StatData>(new HeroEquipStatKeyComparer());

	private Dictionary<CachedHeroKey, CachedStatData> _cachedHeroDic = new Dictionary<CachedHeroKey, CachedStatData>(new CachedHeroKeyComparer());

	public CachedHeroInfo GetReferenceHero(int heroIndex, int heroStar = 0)
	{
		CreatureData data = GameRoot.Instance.CreatureDataContainer.GetData(heroIndex);
		if (heroStar == 0)
		{
			heroStar = data?.StartHeroStar ?? 1;
		}
		Tuple<int, int> key = new Tuple<int, int>(heroIndex, heroStar);
		if (!_referenceHeroDic.TryGetValue(key, out var value))
		{
			int maxLevel = GameRoot.Instance.CreatureLevelDataContainer.MaxLevel;
			int valueInt = GameRoot.Instance.ConstantDataContainer.GetValueInt("MaxSkillExtend");
			value = new CachedHeroInfo();
			value.HeroIndex = heroIndex;
			value.Level = (short)maxLevel;
			value.Star = (sbyte)heroStar;
			for (int i = 1; i <= 5; i++)
			{
				value.SetSkillLevel(i, maxLevel);
				value.SetSkillExtend(i, valueInt);
			}
			_referenceHeroDic.Add(key, value);
		}
		return value;
	}

	public StatData GetHeroStat(HeroInfo info)
	{
		HeroKey key = new HeroKey(info);
		if (!_heroDic.TryGetValue(key, out var value))
		{
			value = StatCalculator.GetHeroStat(info);
			_heroDic.Add(key, value);
		}
		return value;
	}

	public StatData GetEquipItemStat(EquipItemInfo info)
	{
		EquipItemKey key = new EquipItemKey(info);
		if (!_equipItemDic.TryGetValue(key, out var value))
		{
			value = StatCalculator.GetEquipItemStatData(info);
			_equipItemDic.Add(key, value);
		}
		return value;
	}

	public StatData GetHeroEquipStat(CachedHeroInfo info)
	{
		HeroEquipStatKey key = new HeroEquipStatKey(info);
		if (!_heroEquipStatDic.TryGetValue(key, out var value))
		{
			value = StatCalculator.GetEquipStat(info);
			_heroEquipStatDic.Add(key, value);
		}
		return value;
	}

	public void InvalidateCache()
	{
		_cachedHeroDic.Clear();
	}

	public CachedStatData GetCachedHeroStatDetail(CachedHeroInfo info, StatCalculator.HeroStatCalculationFlag calculationFlag = StatCalculator.HeroStatCalculationFlag.All)
	{
		CachedHeroKey key = new CachedHeroKey(info, calculationFlag);
		CachedStatData value = null;
		if (!_cachedHeroDic.TryGetValue(key, out value))
		{
			Dictionary<int, int> dictionary = new Dictionary<int, int>();
			Dictionary<int, int> dictionary2 = new Dictionary<int, int>();
			StatData equippedHeroStat = StatCalculator.GetEquippedHeroStat(info, calculationFlag, null, dictionary, dictionary2);
			value = new CachedStatData
			{
				StatData = equippedHeroStat,
				AddedStatDictionary = dictionary,
				MultedStatDictionary = dictionary2
			};
			_cachedHeroDic.Add(key, value);
		}
		return value;
	}

	public StatData GetCachedHeroStat(CachedHeroInfo info, StatCalculator.HeroStatCalculationFlag calculationFlag = StatCalculator.HeroStatCalculationFlag.All)
	{
		return GetCachedHeroStatDetail(info, calculationFlag)?.StatData;
	}

	[Conditional("UNITY_EDITOR")]
	private void VerifyHeroStat(StatData statData, HeroInfo info)
	{
		StatData heroStat = StatCalculator.GetHeroStat(info);
		EnumLookup<StatType>.ValueData[] valueArray = EnumLookup<StatType>.ValueArray;
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(valueArray); i++)
		{
			EnumLookup<StatType>.ValueData valueData = valueArray[i];
			long stat = heroStat.GetStat(valueData.EnumValue);
			long stat2 = statData.GetStat(valueData.EnumValue);
			if (stat != stat2)
			{
				VespaDebug.LogError("Invalid Stat Cache");
				break;
			}
		}
	}

	[Conditional("UNITY_EDITOR")]
	private void VerifyEquipItemStat(StatData statData, EquipItemInfo info)
	{
		StatData equipItemStatData = StatCalculator.GetEquipItemStatData(info);
		EnumLookup<StatType>.ValueData[] valueArray = EnumLookup<StatType>.ValueArray;
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(valueArray); i++)
		{
			EnumLookup<StatType>.ValueData valueData = valueArray[i];
			long stat = equipItemStatData.GetStat(valueData.EnumValue);
			long stat2 = statData.GetStat(valueData.EnumValue);
			if (stat != stat2)
			{
				VespaDebug.LogError("Invalid Stat Cache");
				break;
			}
		}
	}

	[Conditional("UNITY_EDITOR")]
	private void VerifyHeroEquipStat(StatData statData, CachedHeroInfo info)
	{
		StatData equipStat = StatCalculator.GetEquipStat(info);
		EnumLookup<StatType>.ValueData[] valueArray = EnumLookup<StatType>.ValueArray;
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(valueArray); i++)
		{
			EnumLookup<StatType>.ValueData valueData = valueArray[i];
			long stat = equipStat.GetStat(valueData.EnumValue);
			long stat2 = statData.GetStat(valueData.EnumValue);
			if (stat != stat2)
			{
				VespaDebug.LogError("Invalid Stat Cache");
				break;
			}
		}
	}

	[Conditional("UNITY_EDITOR")]
	private void VerifyCachedHeroStat(StatData statData, CachedHeroInfo info, StatCalculator.HeroStatCalculationFlag calculationFlag = StatCalculator.HeroStatCalculationFlag.All)
	{
		StatData equippedHeroStat = StatCalculator.GetEquippedHeroStat(info, calculationFlag);
		EnumLookup<StatType>.ValueData[] valueArray = EnumLookup<StatType>.ValueArray;
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(valueArray); i++)
		{
			EnumLookup<StatType>.ValueData valueData = valueArray[i];
			long stat = equippedHeroStat.GetStat(valueData.EnumValue);
			long stat2 = statData.GetStat(valueData.EnumValue);
			if (stat != stat2)
			{
				VespaDebug.LogError("Invalid Stat Cache");
				break;
			}
		}
	}
}
