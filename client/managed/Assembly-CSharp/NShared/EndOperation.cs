namespace NShared;

[BattleCallback]
public sealed class EndOperation : BaseBattleCallback
{
	public int CreatureUid { get; set; }

	public long OperationUid { get; set; }

	public override bool IsEquals(BaseBattleCallback callbackBase)
	{
		if (!(callbackBase is EndOperation endOperation))
		{
			return false;
		}
		if (base.GlobalTimestamp == endOperation.GlobalTimestamp && CreatureUid == endOperation.CreatureUid)
		{
			return OperationUid == endOperation.OperationUid;
		}
		return false;
	}
}
