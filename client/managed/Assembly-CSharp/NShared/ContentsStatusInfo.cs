namespace NShared;

[JsonSerializable(Javascript = true)]
public class ContentsStatusInfo
{
	public string ContentsType { get; set; }

	public string ContentsSubType { get; set; }

	public string Status { get; set; }
}
