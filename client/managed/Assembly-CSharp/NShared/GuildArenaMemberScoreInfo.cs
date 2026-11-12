namespace NShared;

[JsonSerializable(Javascript = true)]
public class GuildArenaMemberScoreInfo : GuildMemberServerInfo
{
	public int TotalBattleCount { get; set; }

	public int ScoreRank { get; set; }

	public int MatchWinCount { get; set; }

	public int DefenceWinCount { get; set; }

	public int WinCount { get; set; }

	public int LoseCount { get; set; }

	public int SessionNumber { get; set; }

	public int SessionScore { get; set; }
}
