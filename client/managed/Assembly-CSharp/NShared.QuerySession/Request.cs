namespace NShared.QuerySession;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "user/query_session";

	public long AccountId { get; set; }
}
