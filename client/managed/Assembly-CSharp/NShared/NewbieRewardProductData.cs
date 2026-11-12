using MessagePack;

namespace NShared;

[MessagePackObject(false)]
public class NewbieRewardProductData : IDataKeySelector<int>
{
	[Key(0)]
	public int Index { get; set; }

	[Key(1)]
	public NewbieRewardType RewardType { get; set; }

	[Key(2)]
	public NewbieRewardPurchaseType PurchaseType { get; set; }

	[Key(3)]
	public int PayShopProductIndex { get; set; }

	[Key(4)]
	public int GroupIndex { get; set; }

	[Key(5)]
	public int ShowIndex { get; set; }

	[Key(6)]
	public int ProductGroupIndex { get; set; }

	int IDataKeySelector<int>.Key => Index;

	public override string ToString()
	{
		return "Index=" + Index;
	}
}
