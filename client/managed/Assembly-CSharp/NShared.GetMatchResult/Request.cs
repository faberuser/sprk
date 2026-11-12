namespace NShared.GetMatchResult;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "match/get_match_result";

	public long MatchUid { get; set; }
}
