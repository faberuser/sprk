using MessagePack;

namespace NShared;

[MessagePackObject(false)]
public class GachaSelectShopData : IDataKeySelector<int>
{
	[Key(0)]
	public int Index { get; set; }

	[Key(1)]
	public string CategoryGroup { get; set; }

	[Key(2)]
	public string CategoryName { get; set; }

	[Key(3)]
	public string ItemGroupCode { get; set; }

	[Key(4)]
	public string ItemGroupCodeNameKeyOther { get; set; }

	[Key(5)]
	public string ItemGroupCodeNameKeyApple { get; set; }

	[Key(6)]
	public int ProductIndexGem { get; set; }

	[Key(7)]
	public int ProductIndexCash { get; set; }

	[Key(8)]
	public int Rate { get; set; }

	int IDataKeySelector<int>.Key => Index;
}
