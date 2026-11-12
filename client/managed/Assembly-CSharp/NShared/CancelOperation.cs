namespace NShared;

[BattleCallback]
public sealed class CancelOperation : BaseBattleCallback
{
	public enum EType
	{
		ReservedOperation,
		DeadCreature,
		OutOfRange,
		ConditionFail
	}

	public int CreatureUid { get; set; }

	public long OperationUid { get; set; }

	public EType CancelType { get; set; }

	public override bool IsEquals(BaseBattleCallback callbackBase)
	{
		if (!(callbackBase is CancelOperation cancelOperation))
		{
			return false;
		}
		if (base.GlobalTimestamp == cancelOperation.GlobalTimestamp && CreatureUid == cancelOperation.CreatureUid && OperationUid == cancelOperation.OperationUid)
		{
			return CancelType == cancelOperation.CancelType;
		}
		return false;
	}
}
