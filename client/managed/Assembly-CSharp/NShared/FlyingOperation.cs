namespace NShared;

[BattleCallback]
public sealed class FlyingOperation : BaseBattleCallback
{
	public long OperationUid { get; set; }

	public int ReserveDurationTimeMs { get; set; }

	public int FlyingDurationTimeMs { get; set; }

	public long TargetCreatureUid { get; set; }

	public override bool IsEquals(BaseBattleCallback callbackBase)
	{
		if (!(callbackBase is FlyingOperation flyingOperation))
		{
			return false;
		}
		if (base.GlobalTimestamp == flyingOperation.GlobalTimestamp && OperationUid == flyingOperation.OperationUid && ReserveDurationTimeMs == flyingOperation.ReserveDurationTimeMs && FlyingDurationTimeMs == flyingOperation.FlyingDurationTimeMs)
		{
			return TargetCreatureUid == flyingOperation.TargetCreatureUid;
		}
		return false;
	}
}
