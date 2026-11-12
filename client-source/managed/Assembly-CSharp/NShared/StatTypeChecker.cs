using System.Collections.Generic;

namespace NShared;

public static class StatTypeChecker
{
	private static Dictionary<StatType, StatType> EqualStatGroup;

	static StatTypeChecker()
	{
		EqualStatGroup = new Dictionary<StatType, StatType>(StatTypeComparer.Default);
		EqualStatGroup[StatType.PhysicalAttack] = StatType.MagicalAttack;
		EqualStatGroup[StatType.AddPhysicalAttackR] = StatType.AddMagicalAttackR;
		EqualStatGroup[StatType.PhysicalCriticalChance] = StatType.MagicalCriticalChance;
		EqualStatGroup[StatType.PhysicalCriticalPower] = StatType.MagicalCriticalPower;
		EqualStatGroup[StatType.PhysicalHitChance] = StatType.MagicalHitChance;
		EqualStatGroup[StatType.PhysicalPiercePower] = StatType.MagicalPiercePower;
		EqualStatGroup[StatType.PhysicalDotPower] = StatType.MagicalDotPower;
		EqualStatGroup[StatType.PhysicalIgnoreBlockPower] = StatType.MagicalIgnoreBlockPower;
		EqualStatGroup[StatType.PhysicalIgnoreDefensePower] = StatType.MagicalIgnoreDefensePower;
		EqualStatGroup[StatType.MagicalAttack] = StatType.PhysicalAttack;
		EqualStatGroup[StatType.AddMagicalAttackR] = StatType.AddPhysicalAttackR;
		EqualStatGroup[StatType.MagicalCriticalChance] = StatType.PhysicalCriticalChance;
		EqualStatGroup[StatType.MagicalCriticalPower] = StatType.PhysicalCriticalPower;
		EqualStatGroup[StatType.MagicalHitChance] = StatType.PhysicalHitChance;
		EqualStatGroup[StatType.MagicalPiercePower] = StatType.PhysicalPiercePower;
		EqualStatGroup[StatType.MagicalDotPower] = StatType.PhysicalDotPower;
		EqualStatGroup[StatType.MagicalIgnoreBlockPower] = StatType.PhysicalIgnoreBlockPower;
		EqualStatGroup[StatType.MagicalIgnoreDefensePower] = StatType.PhysicalIgnoreDefensePower;
	}

	public static bool IsContainEqualStatGroup(StatType statType)
	{
		return EqualStatGroup.ContainsKey(statType);
	}

	public static StatType ReverseStatType(StatType statType)
	{
		if (IsContainEqualStatGroup(statType))
		{
			return EqualStatGroup[statType];
		}
		return statType;
	}
}
