namespace NShared.NWorldBoss;

[JsonSerializable(Javascript = true)]
public class WorldBossRankerInfo
{
	public int Rank { get; set; }

	public long AccountId { get; set; }

	public string Nick { get; set; }

	public long Score { get; set; }

	public int AvatarHeroIndex { get; set; }

	public string CountryCode { get; set; }

	public string ServerGroup { get; set; }
}
