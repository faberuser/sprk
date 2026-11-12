namespace NShared.G2M_SyncRecommendDeck;

[Request]
public class Request
{
	public string Host { get; set; }

	public RecommendRankInfo[] RecommendLevelRanks { get; set; }

	public RecommendRankInfo[] RecommendTimeRanks { get; set; }
}
