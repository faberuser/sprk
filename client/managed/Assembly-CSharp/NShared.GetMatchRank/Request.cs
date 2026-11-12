namespace NShared.GetMatchRank;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "match/get_match_rank";

	public long MatchType { get; set; }

	public MatchArenaType ArenaType { get; set; }

	public MatchTierType TierType { get; set; }

	public long RankerAccountId { get; set; }
}
