using NShared.NUtil;

namespace NShared;

[BattlePacket]
public class SetManualMoveReq : BaseCreatureActionReq
{
	public Vector3 Position { get; set; }
}
