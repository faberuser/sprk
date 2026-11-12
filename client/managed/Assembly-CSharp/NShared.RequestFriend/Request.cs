namespace NShared.RequestFriend;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "friend/request_friend";

	public long FriendId { get; set; }

	public FriendInviteType InviteType { get; set; }
}
