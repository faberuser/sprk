namespace NShared;

[JsonSerializable(Javascript = true)]
public class FriendUserInfo : CachedUserInfo
{
	public string InvitedTime { get; set; }

	public string LastOnlineTime { get; set; }
}
