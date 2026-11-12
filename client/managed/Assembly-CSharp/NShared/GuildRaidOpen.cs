namespace NShared;

[JsonSerializable(Javascript = true)]
public class GuildRaidOpen
{
	public int GuildId { get; set; }

	public EGuidRaidOpenStatus Status { get; set; }

	public string OpenedTime { get; set; }
}
