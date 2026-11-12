namespace NShared.QueryNick;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "user/query_nick";

	public string Nick { get; set; }
}
