namespace NShared.NMessage;

[MessagePacket]
public class ChangeChannelReq : BaseRequest
{
	public int ChannelNo { get; set; }
}
