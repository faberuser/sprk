namespace NShared;

[BattlePacket]
public class EclipseContinuousBattleReq : BaseRequest
{
	public bool IsContinuousBattle { get; set; }
}
