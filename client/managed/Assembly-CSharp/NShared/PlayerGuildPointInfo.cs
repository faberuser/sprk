namespace NShared;

[JsonSerializable(Javascript = true)]
public class PlayerGuildPointInfo
{
	public long GuildPoint { get; set; }

	public long DailyAccGuildPoint { get; set; }

	public string DailyAccGuildPointResetTime { get; set; }
}
