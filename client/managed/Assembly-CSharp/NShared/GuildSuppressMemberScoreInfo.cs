namespace NShared;

[JsonSerializable(Javascript = true)]
public class GuildSuppressMemberScoreInfo
{
	public long GuildId { get; set; }

	public string ServerGroup { get; set; }

	public long AccountId { get; set; }

	public string NickName { get; set; }

	public int AvatarHeroIndex { get; set; }

	public long Score { get; set; }
}
