using System.Runtime.CompilerServices;

namespace NShared.BuyPayShopProduct;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	[CompilerGenerated]
	private long _003CIndex_003Ek__BackingField;

	public int SprkQuantity;

	string IServiceRequest.ServiceName => "shop/buy_payshop_product";

	public long Index
	{
		[CompilerGenerated]
		get
		{
			return _003CIndex_003Ek__BackingField;
		}
		[CompilerGenerated]
		set
		{
			_003CIndex_003Ek__BackingField = value;
			SprkQuantity = SprkSpecialShop.TakeQuantity(value);
		}
	}
}
