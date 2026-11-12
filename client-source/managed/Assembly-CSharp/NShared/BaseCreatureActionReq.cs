namespace NShared;

[BattlePacket]
public class BaseCreatureActionReq : BaseRequest
{
	public int CreatureUid { get; set; }
}
