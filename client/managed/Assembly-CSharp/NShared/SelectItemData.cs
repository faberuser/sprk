using MessagePack;

namespace NShared;

[MessagePackObject(false)]
public class SelectItemData : IItemSubData, IDataKeySelector<string>
{
	[Key(0)]
	public string ItemCode { get; set; }

	[Key(1)]
	public int[] ItemIndices { get; set; }

	[Key(2)]
	public SelectItemPopupType SelectType { get; set; }

	[Key(3)]
	public int SelectAbleCount { get; set; }

	string IDataKeySelector<string>.Key => ItemCode;

	public override string ToString()
	{
		return "ItemCode=" + ItemCode;
	}
}
