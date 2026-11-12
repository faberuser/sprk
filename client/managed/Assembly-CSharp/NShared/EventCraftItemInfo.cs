namespace NShared;

[JsonSerializable(Javascript = true)]
public class EventCraftItemInfo : IAccountAttrKey<int>
{
	int IAccountAttrKey<int>.Key => Index;

	public int Index { get; set; }

	public string ShowGroup { get; set; }

	public int CraftCount { get; set; }

	public string UpdatedTime { get; set; }
}
