namespace NShared;

[JsonSerializable(Javascript = true)]
public class SelectShopNoticeInfo
{
	public string EndTime { get; set; }

	public string OpenCategory { get; set; }

	public string[] CategoryGroups { get; set; }

	public string[] CategoryNameKeys { get; set; }
}
