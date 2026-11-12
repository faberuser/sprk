namespace NShared;

[BattleCallback]
public sealed class BeginOperation : BaseBattleCallback
{
	public int CreatureUid { get; set; }

	public long OperationUid { get; set; }

	public int DurationMs { get; set; }

	public override bool IsEquals(BaseBattleCallback callbackBase)
	{
		if (!(callbackBase is BeginOperation beginOperation))
		{
			return false;
		}
		if (base.GlobalTimestamp == beginOperation.GlobalTimestamp && CreatureUid == beginOperation.CreatureUid && OperationUid == beginOperation.OperationUid)
		{
			return DurationMs == beginOperation.DurationMs;
		}
		return false;
	}
}
