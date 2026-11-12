namespace NShared;

[BattlePacket]
public class BattleMessageReq : BaseRequest
{
	public MessageGroupType GroupType { get; set; }

	public long[] ReceiverIds { get; set; }

	public string Message { get; set; }
}
