namespace NShared.NMessage;

[MessagePacket]
public class ChangeFriendReq : BaseRequest
{
	public long[] FriendIds { get; set; }
}
