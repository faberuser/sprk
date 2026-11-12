namespace NShared.NMessage;

[MessagePacket]
public class MessageRes : BaseResponse
{
	public MessageResType Result { get; set; }
}
