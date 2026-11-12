using NShared.NUtil;

namespace NShared.NOperationCondition;

public class CompareStat : IOperationCondition
{
	public enum CompareStatTargetType
	{
		FiredCreature,
		Constant
	}

	bool IOperationCondition.Meet(FiredOperationInfo firedOperationInfo, BattleCreature targetCreature, string[] values)
	{
		StatType valueEnum = ArgParser.GetValueEnum(values, 0, StatType.None);
		CheckType valueEnum2 = ArgParser.GetValueEnum(values, 1, CheckType.Equal);
		int valueInt = ArgParser.GetValueInt(values, 2, 1000);
		switch (ArgParser.GetValueEnum(values, 3, CompareStatTargetType.FiredCreature))
		{
		case CompareStatTargetType.FiredCreature:
		{
			long checkValue = ((firedOperationInfo.FiredCreature != null) ? firedOperationInfo.FiredCreature.StatController.GetFinalStat(valueEnum) : 0);
			long num = targetCreature?.StatController.GetFinalStat(valueEnum) ?? 0;
			num = num * valueInt / 1000;
			return Condition.CheckCompareResult(valueEnum2, checkValue, num);
		}
		case CompareStatTargetType.Constant:
		{
			long testValue = targetCreature?.StatController.GetFinalStat(valueEnum) ?? 0;
			return Condition.CheckCompareResult(valueEnum2, valueInt, testValue);
		}
		default:
			return false;
		}
	}
}
