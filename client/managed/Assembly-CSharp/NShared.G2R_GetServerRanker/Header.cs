namespace NShared.G2R_GetServerRanker;

[JsonSerializable(Javascript = true)]
public class Header
{
	public string Host { get; set; }

	public string Type { get; set; }

	public string Group { get; set; }
}
