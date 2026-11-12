namespace NShared.NBattleTacticEvent;

public class OperationPreBeginEvent : IBattleTacticEvent
{
	public BattleCreature Owner;

	public OperationInfo OperationInfo;

	public OperationResult TacticResult;
}
