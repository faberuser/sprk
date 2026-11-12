using MessagePack;

namespace NShared;

[MessagePackObject(false)]
public class ShopItemData : IDataKeySelector<ShopAndIndexKey>
{
	ShopAndIndexKey IDataKeySelector<ShopAndIndexKey>.Key => new ShopAndIndexKey(ShopIndex, Index);

	[Key(0)]
	public int ShopIndex { get; set; }

	[Key(1)]
	public int Index { get; set; }

	[Key(2)]
	public int GroupIndex { get; set; }

	[Key(3)]
	public int ShowIndex { get; set; }

	[Key(4)]
	public string ItemCode { get; set; }

	[Key(5)]
	public int Count { get; set; }

	[Key(6)]
	public int Star { get; set; }

	[Key(7)]
	public int PurchasableCount { get; set; }

	[Key(8)]
	public bool DailyReset { get; set; }

	[Key(9)]
	public bool WeeklyReset { get; set; }

	[Key(10)]
	public int[] MonthlyResetDate { get; set; }

	[Key(11)]
	public string Condition { get; set; }

	[Key(12)]
	public ShopCostType BuyCostTypeForItem { get; set; }

	[Key(13)]
	public int TargetPrice { get; set; }

	public override string ToString()
	{
		return "Index=" + Index;
	}
}
