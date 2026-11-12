namespace NShared;

[JsonSerializable(Javascript = true)]
public class GuildRaidPlayingInfo
{
	public long PlayingAccountId { get; set; }

	public string TouchedTime { get; set; }
}
