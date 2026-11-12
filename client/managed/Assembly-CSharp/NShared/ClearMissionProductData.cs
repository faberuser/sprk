using MessagePack;

namespace NShared;

[MessagePackObject(false)]
public class ClearMissionProductData : IDataKeySelector<int>
{
	[Key(0)]
	public int Index { get; set; }

	[Key(1)]
	public string MissionName { get; set; }

	[Key(2)]
	public string MissionInfo { get; set; }

	[Key(3)]
	public ClearMissionType MissionType { get; set; }

	[Key(4)]
	public string[] MissionOpenCondition { get; set; }

	[Key(5)]
	public int PayShopProductIndex { get; set; }

	[Key(6)]
	public ClearMissionPurchaseType PurchaseType { get; set; }

	[Key(7)]
	public string ProductImagePath { get; set; }

	[Key(8)]
	public int ProductGroupIndex { get; set; }

	[Key(9)]
	public int ShowIndex { get; set; }

	int IDataKeySelector<int>.Key => Index;

	public override string ToString()
	{
		return "Index=" + Index;
	}
}
