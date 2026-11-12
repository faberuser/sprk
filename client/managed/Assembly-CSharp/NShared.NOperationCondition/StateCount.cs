using NShared.NUtil;

namespace NShared.NOperationCondition;

public class StateCount : IOperationCondition
{
	bool IOperationCondition.Meet(FiredOperationInfo firedOperationInfo, BattleCreature targetCreature, string[] values)
	{
		string valueString = ArgParser.GetValueString(values, 0);
		int valueInt = ArgParser.GetValueInt(values, 1);
		CheckType valueEnum = ArgParser.GetValueEnum(values, 2, CheckType.More);
		bool valueBool = ArgParser.GetValueBool(values, 3);
		if (string.IsNullOrEmpty(valueString))
		{
			return false;
		}
		int testValue = (valueBool ? firedOperationInfo.FiredCreature : targetCreature)?.StateController.GetStateStackCount(valueString) ?? 0;
		return Condition.CheckCompareResult(valueEnum, valueInt, testValue);
	}
}
