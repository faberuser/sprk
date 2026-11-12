namespace NShared.NOperationCondition;

public class HpRatioMoreOrEqual : IOperationCondition
{
	bool IOperationCondition.Meet(FiredOperationInfo firedOperationInfo, BattleCreature targetCreature, string[] values)
	{
		int valueInt = ArgParser.GetValueInt(values, 0);
		if (targetCreature != null)
		{
			return targetCreature.StatController.HpRatioM >= valueInt;
		}
		return false;
	}
}
