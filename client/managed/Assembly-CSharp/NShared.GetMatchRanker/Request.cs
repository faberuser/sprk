namespace NShared.GetMatchRanker;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "match/get_match_ranker";

	public MatchArenaType ArenaType { get; set; }

	public MatchTierType TierType { get; set; }

	public int StartRank { get; set; }

	public int EndRank { get; set; }
}
