namespace NShared;

public static class StatUtility
{
	public static bool IsPercentageStat(StatType statType)
	{
		if (statType == StatType.PhysicalCriticalChance || statType == StatType.MagicalCriticalChance)
		{
			return true;
		}
		return false;
	}
}
