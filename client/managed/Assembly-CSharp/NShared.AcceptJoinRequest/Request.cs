namespace NShared.AcceptJoinRequest;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "guild/accept_join_request";

	public long AccountId { get; set; }
}
