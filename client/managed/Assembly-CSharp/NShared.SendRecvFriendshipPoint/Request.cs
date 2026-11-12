namespace NShared.SendRecvFriendshipPoint;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "friend/send_recv_friendship_point";

	public long[] SendFriendIds { get; set; }

	public long[] RecvFriendIds { get; set; }
}
