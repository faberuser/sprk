namespace NShared.RejectFriend;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "friend/reject_friend";

	public long FriendId { get; set; }
}
