namespace NShared.NWorldBoss;

[JsonSerializable(Javascript = true)]
public class WorldBossServerTotalRankingInfo
{
	public int Rank { get; set; }

	public string ServerGroup { get; set; }

	public long Score { get; set; }
}
