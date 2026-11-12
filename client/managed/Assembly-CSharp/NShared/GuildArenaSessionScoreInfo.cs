namespace NShared;

[JsonSerializable(Javascript = true)]
public class GuildArenaSessionScoreInfo
{
	public int SessionIndex { get; set; }

	public int SeasonIndex { get; set; }

	public string ServerGroup { get; set; }

	public long GuildId { get; set; }

	public string EnemyServerGroup { get; set; }

	public long EnemyGuildId { get; set; }

	public int SessionScore { get; set; }

	public int AddSeasonScore { get; set; }

	public int WinResult { get; set; }

	public long MVPMemberId { get; set; }

	public int MVPSessionScore { get; set; }
}
