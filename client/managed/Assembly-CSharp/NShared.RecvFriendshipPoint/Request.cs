namespace NShared.RecvFriendshipPoint;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "friend/recv_friendship_point";

	public long[] FriendIds { get; set; }
}
