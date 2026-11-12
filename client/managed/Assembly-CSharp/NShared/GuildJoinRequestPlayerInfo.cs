namespace NShared;

[JsonSerializable(Javascript = true)]
public class GuildJoinRequestPlayerInfo : IAccountAttrKey<long>
{
	long IAccountAttrKey<long>.Key => AccountId;

	public long AccountId { get; set; }

	public byte TeamLevel { get; set; }

	public string Nick { get; set; }

	public int AvatarHeroIndex { get; set; }

	public string LastLoginTime { get; set; }
}
