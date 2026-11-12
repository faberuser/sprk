namespace NShared.NOperationCondition;

public class Race : IOperationCondition
{
	bool IOperationCondition.Meet(FiredOperationInfo firedOperationInfo, BattleCreature targetCreature, string[] values)
	{
		CreatureRaceType[] valueEnums = ArgParser.GetValueEnums<CreatureRaceType>(values, 0);
		bool valueBool = ArgParser.GetValueBool(values, 1);
		if (valueEnums == null || valueEnums.Length == 0)
		{
			return false;
		}
		BattleCreature battleCreature = (valueBool ? firedOperationInfo.FiredCreature : targetCreature);
		for (int i = 0; i < valueEnums.Length; i++)
		{
			if (valueEnums[i] == battleCreature.CreatureData.Race)
			{
				return true;
			}
		}
		return false;
	}
}
