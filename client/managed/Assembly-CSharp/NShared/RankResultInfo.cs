namespace NShared;

[JsonSerializable(Javascript = true)]
public class RankResultInfo
{
	public int TierRank { get; set; }

	public int TotalRank { get; set; }

	public int TotalRankerCount { get; set; }
}
