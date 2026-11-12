using NShared.NUtil;

namespace NShared.NOperationCondition;

public class HeroEquipTypeCount : IOperationCondition
{
	bool IOperationCondition.Meet(FiredOperationInfo firedOperationInfo, BattleCreature targetCreature, string[] values)
	{
		EquipItemEquipType valueEnum = ArgParser.GetValueEnum(values, 0, EquipItemEquipType.None);
		int valueInt = ArgParser.GetValueInt(values, 1);
		CheckType valueEnum2 = ArgParser.GetValueEnum(values, 2, CheckType.Equal);
		int testValue = (ArgParser.GetValueBool(values, 3) ? firedOperationInfo.FiredCreature : targetCreature)?.GetEquipTypeCount(valueEnum) ?? 0;
		return Condition.CheckCompareResult(valueEnum2, valueInt, testValue);
	}
}
