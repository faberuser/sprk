namespace NShared.NOperationCondition;

public class Random : IOperationCondition
{
	bool IOperationCondition.Meet(FiredOperationInfo firedOperationInfo, BattleCreature targetCreature, string[] values)
	{
		int valueInt = ArgParser.GetValueInt(values, 0);
		if (valueInt <= 0)
		{
			return false;
		}
		return firedOperationInfo.FiredCreature.NextRandom(1000, "NOperationCondition.Random") < valueInt;
	}
}
