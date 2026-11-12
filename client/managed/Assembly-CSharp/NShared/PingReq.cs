namespace NShared;

[BattlePacket]
public class PingReq : BaseRequest
{
	public long ClientTimeMs { get; set; }
}
