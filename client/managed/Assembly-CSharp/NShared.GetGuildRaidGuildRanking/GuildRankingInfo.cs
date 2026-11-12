namespace NShared.GetGuildRaidGuildRanking;

[JsonSerializable(Javascript = true)]
public class GuildRankingInfo
{
	public int ChapterIndex { get; set; }

	public int Difficulty { get; set; }

	public int GuildId { get; set; }

	public string GuildName { get; set; }

	public int GuildLogo { get; set; }

	public int ClearTime { get; set; }
}
