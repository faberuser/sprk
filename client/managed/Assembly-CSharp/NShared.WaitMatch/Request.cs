namespace NShared.WaitMatch;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "match/wait_match";

	public bool PlayOfflineMatch { get; set; }

	public bool ReserveCancel { get; set; }
}
