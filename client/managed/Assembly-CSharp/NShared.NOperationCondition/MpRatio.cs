using NShared.NUtil;

namespace NShared.NOperationCondition;

public class MpRatio : IOperationCondition
{
	bool IOperationCondition.Meet(FiredOperationInfo firedOperationInfo, BattleCreature targetCreature, string[] values)
	{
		int valueInt = ArgParser.GetValueInt(values, 0);
		CheckType valueEnum = ArgParser.GetValueEnum(values, 1, CheckType.Equal);
		int testValue = (ArgParser.GetValueBool(values, 2) ? firedOperationInfo.FiredCreature : targetCreature)?.StatController.MpRatioM ?? 0;
		return Condition.CheckCompareResult(valueEnum, valueInt, testValue);
	}
}
