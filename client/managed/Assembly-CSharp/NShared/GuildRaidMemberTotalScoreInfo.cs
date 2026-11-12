namespace NShared;

[JsonSerializable(Javascript = true)]
public class GuildRaidMemberTotalScoreInfo
{
	public long AccountId { get; set; }

	public int GuildId { get; set; }

	public long Score { get; set; }
}
