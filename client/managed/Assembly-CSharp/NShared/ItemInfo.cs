namespace NShared;

[JsonSerializable(Javascript = true)]
public class ItemInfo : IAccountAttrKey<int>, IItemInfo
{
	int IAccountAttrKey<int>.Key => ItemIndex;

	int IItemInfo.ItemIndex => ItemIndex;

	int IItemInfo.ItemCount => Count;

	public int ItemIndex { get; set; }

	public int Count { get; set; }

	public string CreatedTime { get; set; }

	public string Uid { get; set; }

	public byte Locked { get; set; }
}
