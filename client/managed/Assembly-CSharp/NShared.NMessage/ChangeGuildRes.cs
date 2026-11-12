namespace NShared.NMessage;

[MessagePacket]
public class ChangeGuildRes : BaseResponse
{
	public ChangeGuildResType Result { get; set; }
}
