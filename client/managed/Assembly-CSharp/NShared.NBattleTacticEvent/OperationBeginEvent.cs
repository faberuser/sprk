namespace NShared.NBattleTacticEvent;

public class OperationBeginEvent : IBattleTacticEvent
{
	public BattleCreature Owner;

	public OperationInfo OperationInfo;
}
