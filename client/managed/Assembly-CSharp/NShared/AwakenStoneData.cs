using MessagePack;

namespace NShared;

[MessagePackObject(false)]
public class AwakenStoneData : IItemSubData, IDataKeySelector<string>
{
	[Key(0)]
	public string ItemCode { get; set; }

	[Key(1)]
	public ItemType Type { get; set; }

	[Key(2)]
	public int AwakenStar { get; set; }

	[Key(3)]
	public int[] ItemIndices { get; set; }

	[Key(4)]
	public bool IsAllowNPC { get; set; }

	string IDataKeySelector<string>.Key => ItemCode;

	public override string ToString()
	{
		return "ItemCode=" + ItemCode;
	}
}
