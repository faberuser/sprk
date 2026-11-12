namespace NShared.NMessage;

[MessagePacket]
public class ClearMessageRes : BaseResponse
{
	public ClearMessageResType Result { get; set; }
}
