namespace NShared.DumpUser;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "user/dump_user";

	public int AccountId { get; set; }
}
