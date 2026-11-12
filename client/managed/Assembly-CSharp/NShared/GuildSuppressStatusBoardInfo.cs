namespace NShared;

[JsonSerializable(Javascript = true)]
public class GuildSuppressStatusBoardInfo
{
	public GuildInfo GuildInfo { get; set; }

	public GuildSuppressPlayInfo[] GuildSuppressPlayInfos { get; set; }

	public GuildSuppressMemberScoreInfo[] GuildSuppressMemberScores { get; set; }

	public GuildSuppressMemberPlayHistory[] GuildSuppressMemberPlayHistorys { get; set; }
}
