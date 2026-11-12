namespace NShared;

[JsonSerializable(Javascript = true)]
public class GuildArenaSeasonScoreInfo
{
	public int SeasonIndex { get; set; }

	public string ServerGroup { get; set; }

	public long GuildId { get; set; }

	public int TierIndex { get; set; }

	public int Rank { get; set; }

	public int SeasonScore { get; set; }

	public int SeasonWin { get; set; }

	public int SeasonLose { get; set; }

	public int SeasonDraw { get; set; }
}
