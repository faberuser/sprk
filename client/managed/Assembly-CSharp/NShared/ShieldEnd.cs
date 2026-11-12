namespace NShared;

[BattleCallback]
public sealed class ShieldEnd : BaseBattleCallback
{
	public int CreatureUid { get; set; }

	public long OperationUid { get; set; }

	public override bool IsEquals(BaseBattleCallback callbackBase)
	{
		if (!(callbackBase is ShieldEnd shieldEnd))
		{
			return false;
		}
		if (base.GlobalTimestamp == shieldEnd.GlobalTimestamp && CreatureUid == shieldEnd.CreatureUid)
		{
			return OperationUid == shieldEnd.OperationUid;
		}
		return false;
	}
}
