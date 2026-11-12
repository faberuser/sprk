namespace NShared;

[JsonSerializable(Javascript = true)]
public class GuildArenaRankerInfo : GuildServerInfo
{
	public int TierIndex { get; set; }

	public int Rank { get; set; }

	public int SeasonScore { get; set; }

	public int SeasonWin { get; set; }

	public int SeasonLose { get; set; }

	public int SeasonDraw { get; set; }

	public int SessionScore { get; set; }

	public GuildMemberServerInfo[] GuildMembers { get; set; }
}
