namespace NShared;

[JsonSerializable(Javascript = true)]
public class GuildArenaAutoApplyInfo
{
	public string ServerGroup { get; set; }

	public long GuildId { get; set; }

	public int AutoApply { get; set; }
}
