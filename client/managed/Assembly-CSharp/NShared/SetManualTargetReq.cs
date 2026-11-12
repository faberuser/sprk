namespace NShared;

[BattlePacket]
public class SetManualTargetReq : BaseCreatureActionReq
{
	public int TargetCreatureUid { get; set; }
}
