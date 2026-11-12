using NShared.NUtil;

namespace NShared;

[BattleCallback]
public sealed class BeginMove : BaseBattleCallback
{
	public int CreatureUid { get; set; }

	public Vector3 StartPosition { get; set; }

	public Vector3 TargetPosition { get; set; }

	public int TargetOffsetIndex { get; set; }

	public int SpeedMms { get; set; }

	public int TargetCreatureUid { get; set; }

	public int ApproachDistanceMm { get; set; }

	public long OperationUid { get; set; }

	public override bool IsEquals(BaseBattleCallback callbackBase)
	{
		if (!(callbackBase is BeginMove beginMove))
		{
			return false;
		}
		if (base.GlobalTimestamp == beginMove.GlobalTimestamp && CreatureUid == beginMove.CreatureUid && OperationUid == beginMove.OperationUid && StartPosition == beginMove.StartPosition && TargetPosition == beginMove.TargetPosition && TargetOffsetIndex == beginMove.TargetOffsetIndex && SpeedMms == beginMove.SpeedMms && TargetCreatureUid == beginMove.TargetCreatureUid)
		{
			return ApproachDistanceMm == beginMove.ApproachDistanceMm;
		}
		return false;
	}
}
