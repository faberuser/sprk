namespace NShared.InspectUser;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "user/inspect_user";

	public long TargetAccountId { get; set; }
}
