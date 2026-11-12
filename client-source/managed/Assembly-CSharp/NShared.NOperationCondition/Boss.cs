namespace NShared.NOperationCondition;

public class Boss : IOperationCondition
{
	bool IOperationCondition.Meet(FiredOperationInfo firedOperationInfo, BattleCreature targetCreature, string[] values)
	{
		return (ArgParser.GetValueBool(values, 0) ? firedOperationInfo.FiredCreature : targetCreature).CreateInfo.IsBoss;
	}
}
