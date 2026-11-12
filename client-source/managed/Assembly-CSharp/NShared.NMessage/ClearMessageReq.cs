namespace NShared.NMessage;

[MessagePacket]
public class ClearMessageReq : BaseRequest
{
	public string MessageTypeName { get; set; }

	public long RemoveAccountId { get; set; }
}
