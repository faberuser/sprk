namespace NShared.GetServerRanker;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "global_arena/get_server_ranker";

	public MatchArenaType ArenaType { get; set; }

	public MatchTierType TierType { get; set; }

	public int StartRank { get; set; }

	public int EndRank { get; set; }
}
