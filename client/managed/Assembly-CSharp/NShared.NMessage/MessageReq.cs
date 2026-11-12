namespace NShared.NMessage;

[MessagePacket]
public class MessageReq : BaseRequest
{
	public MessageGroupType GroupType { get; set; }

	public long[] ReceiverIds { get; set; }

	public string Message { get; set; }
}
