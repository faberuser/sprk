using NShared.NUtil;

namespace NShared.NOperationCondition;

public class SummoneeCount : IOperationCondition
{
	bool IOperationCondition.Meet(FiredOperationInfo firedOperationInfo, BattleCreature targetCreature, string[] values)
	{
		int valueInt = ArgParser.GetValueInt(values, 0);
		CheckType valueEnum = ArgParser.GetValueEnum(values, 1, CheckType.Equal);
		bool valueBool = ArgParser.GetValueBool(values, 2, defaultValue: true);
		int testValue = 0;
		BattleCreature firedCreature = firedOperationInfo.FiredCreature;
		if (firedCreature != null)
		{
			testValue = firedOperationInfo.FiredCreature.BattleInstance.CreatureContainer.GetChildCreatureList(firedCreature.Uid, valueBool).Count;
		}
		return Condition.CheckCompareResult(valueEnum, valueInt, testValue);
	}
}
