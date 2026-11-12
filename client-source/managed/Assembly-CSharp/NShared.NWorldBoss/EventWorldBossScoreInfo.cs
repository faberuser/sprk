namespace NShared.NWorldBoss;

[JsonSerializable(Javascript = true)]
public class EventWorldBossScoreInfo
{
	public long AccountId { get; set; }

	public int WorldBossIndex { get; set; }

	public int Season { get; set; }

	public long Score { get; set; }

	public string Nick { get; set; }

	public int AvatarHeroIndex { get; set; }
}
