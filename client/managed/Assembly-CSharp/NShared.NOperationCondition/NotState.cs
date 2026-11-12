namespace NShared.NOperationCondition;

public class NotState : IOperationCondition
{
	bool IOperationCondition.Meet(FiredOperationInfo firedOperationInfo, BattleCreature targetCreature, string[] values)
	{
		string valueString = ArgParser.GetValueString(values, 0);
		bool valueBool = ArgParser.GetValueBool(values, 1);
		if (string.IsNullOrEmpty(valueString))
		{
			return false;
		}
		BattleCreature battleCreature = (valueBool ? firedOperationInfo.FiredCreature : targetCreature);
		if (battleCreature != null)
		{
			return !battleCreature.StateController.IsInState(valueString);
		}
		return false;
	}
}
