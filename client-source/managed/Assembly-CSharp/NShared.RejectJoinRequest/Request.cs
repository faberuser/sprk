namespace NShared.RejectJoinRequest;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "guild/reject_join_request";

	public long AccountId { get; set; }
}
