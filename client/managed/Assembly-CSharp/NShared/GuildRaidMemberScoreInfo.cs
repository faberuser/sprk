namespace NShared;

[JsonSerializable(Javascript = true)]
public class GuildRaidMemberScoreInfo
{
	public long AccountId { get; set; }

	public long Score { get; set; }
}
