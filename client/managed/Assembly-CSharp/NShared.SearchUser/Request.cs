namespace NShared.SearchUser;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "user/search_user";

	public string SearchNick { get; set; }
}
