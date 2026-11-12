namespace NShared.RegisterMatch;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "match/register_match";

	public MatchArenaType ArenaType { get; set; }

	public MatchDuelType DuelType { get; set; }

	public int[] HeroIndices { get; set; }

	public int LeaderHeroIndex { get; set; }

	public int Rank { get; set; }

	public bool SingleTest { get; set; }

	public long GroupId { get; set; }

	public bool IsRematch { get; set; }

	public bool OpponentIsNpc { get; set; }

	public string OpponentServerGroup { get; set; }

	public long OpponentAccountId { get; set; }

	public int OpponentTierIndex { get; set; }

	public int[] OpponentHeroIndices { get; set; }

	public int OpponentLeaderHeroIndex { get; set; }
}
