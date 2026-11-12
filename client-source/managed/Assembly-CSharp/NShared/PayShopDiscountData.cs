using MessagePack;

namespace NShared;

[MessagePackObject(false)]
public class PayShopDiscountData : IDataKeySelector<int>
{
	[Key(0)]
	public int ProductIndex { get; set; }

	[Key(1)]
	public int GroupIndex { get; set; }

	[Key(2)]
	public string TitleKey { get; set; }

	[Key(3)]
	public int AnimationIndex { get; set; }

	int IDataKeySelector<int>.Key => ProductIndex;
}
