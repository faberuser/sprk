namespace NShared;

[JsonSerializable(Javascript = true)]
public class GuildMemberInfo : CachedUserInfo, IAccountAttrKey<long>
{
	long IAccountAttrKey<long>.Key => base.AccountId;

	public byte Rank { get; set; }

	public int AccActivityPoint { get; set; }

	public int AccSuppressPoint { get; set; }

	public int AccWood { get; set; }

	public int AccStone { get; set; }

	public int AccMetal { get; set; }

	public byte RaidEnterCount { get; set; }

	public byte ContributeCount { get; set; }

	public byte ReqGuildSkill { get; set; }

	public int BidRaidIndex { get; set; }

	public int BidRaidRewardItemIndex { get; set; }

	public string BidRaidRewardTime { get; set; }

	public bool IsMaster()
	{
		return Rank == 1;
	}

	public bool IsAdmin()
	{
		return Rank == 2;
	}
}
