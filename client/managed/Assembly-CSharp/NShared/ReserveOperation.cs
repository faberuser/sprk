namespace NShared;

[BattleCallback]
public sealed class ReserveOperation : BaseBattleCallback
{
	public long OperationUid { get; set; }

	public int FixedDelayTimeMs { get; set; }

	public int DurationTimeMs { get; set; }

	public override bool IsEquals(BaseBattleCallback callbackBase)
	{
		if (!(callbackBase is ReserveOperation reserveOperation))
		{
			return false;
		}
		if (base.GlobalTimestamp == reserveOperation.GlobalTimestamp && OperationUid == reserveOperation.OperationUid && FixedDelayTimeMs == reserveOperation.FixedDelayTimeMs)
		{
			return DurationTimeMs == reserveOperation.DurationTimeMs;
		}
		return false;
	}
}
