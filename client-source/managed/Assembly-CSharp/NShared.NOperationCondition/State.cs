namespace NShared.NOperationCondition;

public class State : IOperationCondition
{
	bool IOperationCondition.Meet(FiredOperationInfo firedOperationInfo, BattleCreature targetCreature, string[] values)
	{
		string valueString = ArgParser.GetValueString(values, 0);
		bool valueBool = ArgParser.GetValueBool(values, 1);
		if (string.IsNullOrEmpty(valueString))
		{
			return false;
		}
		return (valueBool ? firedOperationInfo.FiredCreature : targetCreature)?.StateController.IsInState(valueString) ?? false;
	}
}
