namespace NShared;

[JsonSerializable(Javascript = true)]
public class GuildSuppressBanInfo
{
	public int SeasonIndex { get; set; }

	public int SessionIndex { get; set; }
}
