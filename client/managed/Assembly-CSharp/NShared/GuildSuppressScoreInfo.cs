namespace NShared;

[JsonSerializable(Javascript = true)]
public class GuildSuppressScoreInfo
{
	public int GuildId { get; set; }

	public long AccountId { get; set; }

	public int Rank { get; set; }

	public long TotalDamage { get; set; }
}
