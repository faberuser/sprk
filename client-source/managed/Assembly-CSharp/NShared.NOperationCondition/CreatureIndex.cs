namespace NShared.NOperationCondition;

public class CreatureIndex : IOperationCondition
{
	bool IOperationCondition.Meet(FiredOperationInfo firedOperationInfo, BattleCreature targetCreature, string[] values)
	{
		if (values == null || values.Length == 0)
		{
			return false;
		}
		for (int i = 0; i < values.Length; i++)
		{
			int valueInt = ArgParser.GetValueInt(values, i);
			if (valueInt != 0 && targetCreature != null && targetCreature.CreatureData.Index == valueInt)
			{
				return true;
			}
		}
		return false;
	}
}
