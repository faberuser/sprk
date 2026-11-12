namespace NShared;

[JsonSerializable(Javascript = true)]
public class GuildSuppressGroupRankerInfo
{
	public int Rank { get; set; }

	public long GuildId { get; set; }

	public int GuildLogo { get; set; }

	public string GuildName { get; set; }

	public long Score { get; set; }

	public string CountryCode { get; set; }

	public string ServerGroup { get; set; }
}
