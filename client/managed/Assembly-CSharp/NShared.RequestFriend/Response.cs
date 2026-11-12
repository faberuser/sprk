namespace NShared.RequestFriend;

[Response]
public class Response : GenericResponse<ResultType>
{
	public FriendInfo FriendInfo { get; set; }
}
