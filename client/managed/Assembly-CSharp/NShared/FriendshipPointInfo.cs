namespace NShared;

[JsonSerializable(Javascript = true)]
public class FriendshipPointInfo : IAccountAttrKey<long>
{
	long IAccountAttrKey<long>.Key => FriendId;

	public long FriendId { get; set; }

	public string LastSendTime { get; set; }

	public string LastRecvTime { get; set; }
}
