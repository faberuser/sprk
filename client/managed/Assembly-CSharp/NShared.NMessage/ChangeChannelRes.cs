namespace NShared.NMessage;

[MessagePacket]
public class ChangeChannelRes : BaseResponse
{
	public bool Changed { get; set; }
}
