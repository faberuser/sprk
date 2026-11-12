namespace NShared.NMessage;

[MessagePacket]
public class PingRes : BaseResponse
{
	public PingResType Result { get; set; }
}
