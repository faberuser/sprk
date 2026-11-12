namespace NShared;

[JsonSerializable(Javascript = true)]
public class OrdealNodeInfo
{
	public int Index { get; set; }

	public int Floor { get; set; }

	public EOrdealArenaNodeType NodeType { get; set; }

	public int EventIndex { get; set; }

	public int Rank { get; set; }

	public CachedAccountInfo AccountInfo { get; set; }

	public string CountryCode { get; set; }
}
