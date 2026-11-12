namespace NShared;

[JsonSerializable]
public class AssetBaseInfo_Parse
{
	public string AssetName { get; set; }

	public string Category { get; set; }

	public int PriorityValue { get; set; }
}
