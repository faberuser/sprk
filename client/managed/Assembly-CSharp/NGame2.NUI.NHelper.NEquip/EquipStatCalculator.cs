using System;
using System.Collections.Generic;
using NShared;

namespace NGame2.NUI.NHelper.NEquip;

public class EquipStatCalculator
{
	private static Dictionary<int, int> _addedStatContainer = new Dictionary<int, int>();

	private static Dictionary<int, int> _multedStatContainer = new Dictionary<int, int>();

	private static void Reset()
	{
		_addedStatContainer.Clear();
		_multedStatContainer.Clear();
	}

	public static StatData Calculate(EquipItemInfo equipInfo, bool applyExtraStat = false)
	{
		Reset();
		return StatCalculator.GetEquipStat(equipInfo, _addedStatContainer, _multedStatContainer, applyExtraStat);
	}

	public static long GetSumOfAddedStat(IList<StatType> filterList)
	{
		return GetSumOfAdditionalValue(filterList, _addedStatContainer);
	}

	public static long GetSumOfMutedStat(IList<StatType> filterList)
	{
		return GetSumOfAdditionalValue(filterList, _multedStatContainer);
	}

	private static long GetSumOfAdditionalValue(IList<StatType> filterList, Dictionary<int, int> statContainer)
	{
		if (__Extension_Method_ICollection__.COUNT(statContainer) == 0)
		{
			return 0L;
		}
		long num = 0L;
		foreach (StatType filter in filterList)
		{
			int value = 0;
			statContainer.TryGetValue((int)filter, out value);
			num += value;
		}
		return num;
	}

	public static long GetMaxOfAddedStat(IList<StatType> filterList)
	{
		return GetMaxOfAdditionalValue(filterList, _addedStatContainer);
	}

	public static long GetMaxOfMutedStat(IList<StatType> filterList)
	{
		return GetMaxOfAdditionalValue(filterList, _multedStatContainer);
	}

	private static long GetMaxOfAdditionalValue(IList<StatType> filterList, Dictionary<int, int> statContainer)
	{
		if (__Extension_Method_ICollection__.COUNT(statContainer) == 0)
		{
			return 0L;
		}
		long num = 0L;
		foreach (StatType filter in filterList)
		{
			int value = 0;
			statContainer.TryGetValue((int)filter, out value);
			num = Math.Max(num, value);
		}
		return num;
	}
}
