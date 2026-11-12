namespace NShared.SendFriendshipPoint;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "friend/send_friendship_point";

	public long[] FriendIds { get; set; }
}
