namespace NShared.NMessage;

[MessagePacket]
public class LoginReq : BaseRequest
{
	public long AccountId { get; set; }

	public long GuildId { get; set; }

	public long[] FriendIds { get; set; }

	public int ChannelNo { get; set; }
}
