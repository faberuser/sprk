namespace NShared.G2R_GetServerRanker;

[Request]
public class Request
{
	public Header __Header__ { get; set; }

	public MatchArenaType ArenaType { get; set; }

	public MatchTierType TierType { get; set; }

	public int StartRank { get; set; }

	public int EndRank { get; set; }
}
