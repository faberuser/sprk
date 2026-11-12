namespace NShared.CancelMatch;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "match/cancel_match";

	public MatchArenaType ArenaType { get; set; }
}
