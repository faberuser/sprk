namespace NShared;

[JsonSerializable(Javascript = true)]
public class FriendInfo : IAccountAttrKey<long>
{
	long IAccountAttrKey<long>.Key => FriendId;

	public long FriendId { get; set; }

	public string InvitedTime { get; set; }
}
