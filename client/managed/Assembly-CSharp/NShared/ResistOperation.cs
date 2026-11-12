namespace NShared;

[BattleCallback]
public sealed class ResistOperation : BaseBattleCallback
{
	public int CreatureUid { get; set; }

	public long OperationUid { get; set; }

	public string StateType { get; set; }

	public override bool IsEquals(BaseBattleCallback callbackBase)
	{
		if (!(callbackBase is ResistOperation resistOperation))
		{
			return false;
		}
		if (base.GlobalTimestamp == resistOperation.GlobalTimestamp && CreatureUid == resistOperation.CreatureUid && OperationUid == resistOperation.OperationUid)
		{
			return StateType == resistOperation.StateType;
		}
		return false;
	}
}
