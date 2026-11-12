namespace NShared.NMessage;

[MessagePacket]
public class ChangeFriendRes : BaseResponse
{
	public ChangeFriendResType Result { get; set; }
}
