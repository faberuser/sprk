namespace NShared.SearchFriend;

[Response]
public class Response : GenericResponse<ResultType>
{
	public CachedUserInfo[] FriendInfos { get; set; }
}
