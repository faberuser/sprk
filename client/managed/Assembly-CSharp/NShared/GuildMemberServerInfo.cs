namespace NShared;

[JsonSerializable(Javascript = true)]
public class GuildMemberServerInfo : IAccountAttrKey<long>
{
	long IAccountAttrKey<long>.Key => AccountId;

	public long AccountId { get; set; }

	public string ServerGroup { get; set; }

	public byte Rank { get; set; }

	public string Nick { get; set; }

	public int TeamLevel { get; set; }

	public int AvatarHeroIndex { get; set; }

	public string CountryCode { get; set; }

	public bool IsMaster()
	{
		return Rank == 1;
	}

	public bool IsAdmin()
	{
		return Rank == 2;
	}
}
