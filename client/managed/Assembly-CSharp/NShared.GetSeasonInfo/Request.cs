namespace NShared.GetSeasonInfo;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "match/get_season_info";

	public MatchArenaType ArenaType { get; set; }
}
