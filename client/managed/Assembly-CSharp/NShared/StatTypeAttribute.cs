using System;

namespace NShared;

public class StatTypeAttribute : Attribute
{
	public StatAttributeFlag Flag;

	private static StatTypeAttribute GetAttribute(StatType statType)
	{
		return EnumAttributeContainer<StatType, StatTypeAttribute>.Get(statType);
	}

	public static bool IsPositiveStat(StatType statType)
	{
		return GameRoot.Instance.StatAttributeDataContainer.GetData(statType)?.Positive ?? false;
	}

	public static bool IsNegativeStat(StatType statType)
	{
		return GameRoot.Instance.StatAttributeDataContainer.GetData(statType)?.Negative ?? false;
	}

	public static bool IsPercentStat(StatType statType)
	{
		bool flag = GameRoot.Instance.StatAttributeDataContainer.GetData(statType)?.Percent ?? false;
		if (flag && GameRoot.Instance.StatAttenuationDataContainer.GetData(statType) != null)
		{
			flag = false;
		}
		return flag;
	}

	public static bool IsAllowNegativeStat(StatType statType)
	{
		return GameRoot.Instance.StatAttributeDataContainer.GetData(statType)?.AllowNegative ?? false;
	}
}
