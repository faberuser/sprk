using MessagePack;

namespace NShared;

[MessagePackObject(false)]
[JsonSerializable(Javascript = true)]
public class GachaSelectItemData : IDataKeySelector<int>
{
	[Key(0)]
	public int ItemIndex { get; set; }

	[Key(1)]
	public EquipGachaType GachaType { get; set; }

	[Key(2)]
	public int[] GachaIndices { get; set; }

	[Key(3)]
	public NewPayShopCategoryType GachaCategoryType { get; set; }

	int IDataKeySelector<int>.Key => ItemIndex;
}
