namespace NShared.SearchFriend;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "friend/search_friend";

	public string Keyword { get; set; }
}
