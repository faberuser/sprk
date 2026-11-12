namespace NShared;

[JsonSerializable(Javascript = true)]
public class GuildMemberAccActivityPointResultInfo
{
	public int AddValue { get; set; }

	public int NewValue { get; set; }
}
