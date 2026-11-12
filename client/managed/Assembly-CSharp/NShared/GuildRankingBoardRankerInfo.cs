namespace NShared;

[JsonSerializable(Javascript = true)]
public class GuildRankingBoardRankerInfo
{
	public int Rank { get; set; }

	public byte Logo { get; set; }

	public int Level { get; set; }

	public string Name { get; set; }

	public int CurrentMemberCount { get; set; }

	public long Score { get; set; }

	public long GuildId { get; set; }

	public string CountryCode { get; set; }

	public string ServerGroup { get; set; }
}
