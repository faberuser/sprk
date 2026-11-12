namespace NShared;

[JsonSerializable(Javascript = true)]
public class GuildSuppressAutoApplyInfo
{
	public long GuildId { get; set; }

	public string ServerGroup { get; set; }

	public int AutoApplySuppress { get; set; }
}
