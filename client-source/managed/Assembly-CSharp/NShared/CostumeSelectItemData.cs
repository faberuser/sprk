using MessagePack;

namespace NShared;

[MessagePackObject(false)]
public class CostumeSelectItemData : IItemSubData, IDataKeySelector<string>
{
	[Key(0)]
	public string ItemCode { get; set; }

	[Key(1)]
	public string[] CostumeCategories { get; set; }

	[Key(2)]
	public int[] CostumeIndices { get; set; }

	[Key(3)]
	public int AllOwnedRewardIndex { get; set; }

	[Key(4)]
	public string AllOwnedRewardDesc { get; set; }

	[Key(5)]
	public ECostumeSelectItemType CostumeSelectItemType { get; set; }

	string IDataKeySelector<string>.Key => ItemCode;

	public override string ToString()
	{
		return "ItemCode=" + ItemCode;
	}
}
