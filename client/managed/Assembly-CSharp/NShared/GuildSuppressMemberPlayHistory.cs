namespace NShared;

[JsonSerializable(Javascript = true)]
public class GuildSuppressMemberPlayHistory
{
	public long GuildId { get; set; }

	public string ServerGroup { get; set; }

	public long AccountId { get; set; }

	public string NickName { get; set; }

	public string BattleTime { get; set; }

	public long Score { get; set; }

	public int RaidIndex { get; set; }

	public int Level { get; set; }
}
