namespace NShared;

public interface IOperationCondition
{
	bool Meet(FiredOperationInfo firedOperationInfo, BattleCreature targetCreature, string[] values);
}
