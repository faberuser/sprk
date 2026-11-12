using NShared.NUtil;

namespace NShared.NOperationCondition;

public class DebuffCount : IOperationCondition
{
	bool IOperationCondition.Meet(FiredOperationInfo firedOperationInfo, BattleCreature targetCreature, string[] values)
	{
		int valueInt = ArgParser.GetValueInt(values, 0);
		CheckType valueEnum = ArgParser.GetValueEnum(values, 1, CheckType.Equal);
		int num = 0;
		BattleCreature firedCreature = firedOperationInfo.FiredCreature;
		if (firedCreature != null)
		{
			num += firedCreature.StateController.GetNegativeStateCount();
			num += firedCreature.OperationController.GetNegativeOperationCount();
		}
		return Condition.CheckCompareResult(valueEnum, valueInt, num);
	}
}
