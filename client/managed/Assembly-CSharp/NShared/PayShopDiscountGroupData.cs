using MessagePack;

namespace NShared;

[MessagePackObject(false)]
public class PayShopDiscountGroupData : IDataKeySelector<PayShopDiscountGroupKey>
{
	[Key(0)]
	public int GroupIndex { get; set; }

	[Key(1)]
	public int Step { get; set; }

	[Key(2)]
	public int TargetValue { get; set; }

	[Key(3)]
	public int Price { get; set; }

	PayShopDiscountGroupKey IDataKeySelector<PayShopDiscountGroupKey>.Key => new PayShopDiscountGroupKey(GroupIndex, Step);
}
