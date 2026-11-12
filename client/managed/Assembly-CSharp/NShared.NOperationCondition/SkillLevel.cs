using NShared.NUtil;

namespace NShared.NOperationCondition;

public class SkillLevel : IOperationCondition
{
	bool IOperationCondition.Meet(FiredOperationInfo firedOperationInfo, BattleCreature targetCreature, string[] values)
	{
		int valueInt = ArgParser.GetValueInt(values, 0);
		int valueInt2 = ArgParser.GetValueInt(values, 1);
		CheckType valueEnum = ArgParser.GetValueEnum(values, 2, CheckType.More);
		BattleCreature firedCreature = firedOperationInfo.FiredCreature;
		if (firedCreature == null)
		{
			return false;
		}
		int userSkillLevel = firedCreature.GetUserSkillLevel(valueInt);
		return Condition.CheckCompareResult(valueEnum, valueInt2, userSkillLevel);
	}
}
