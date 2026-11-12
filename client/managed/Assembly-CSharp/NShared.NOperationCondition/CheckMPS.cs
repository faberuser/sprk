using NShared.NUtil;

namespace NShared.NOperationCondition;

public class CheckMPS : IOperationCondition
{
	bool IOperationCondition.Meet(FiredOperationInfo firedOperationInfo, BattleCreature targetCreature, string[] values)
	{
		int valueInt = ArgParser.GetValueInt(values, 0);
		int valueInt2 = ArgParser.GetValueInt(values, 1);
		CheckType valueEnum = ArgParser.GetValueEnum(values, 2, CheckType.More);
		long testValue = (ArgParser.GetValueBool(values, 3) ? firedOperationInfo.FiredCreature : targetCreature)?.ManaHistoryTracker.Get_ManaPerUnitTime() ?? 0;
		if (valueEnum == CheckType.In)
		{
			bool num = Condition.CheckCompareResult(CheckType.MoreOrEqual, valueInt, testValue);
			bool flag = Condition.CheckCompareResult(CheckType.LessOrEqual, valueInt2, testValue);
			return num && flag;
		}
		return Condition.CheckCompareResult(valueEnum, valueInt, testValue);
	}
}
