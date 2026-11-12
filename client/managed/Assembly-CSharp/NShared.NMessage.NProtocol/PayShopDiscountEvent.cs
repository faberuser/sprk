namespace NShared.NMessage.NProtocol;

[JsonSerializable]
public class PayShopDiscountEvent
{
	public PayShopDiscountInfo[] PayShopDiscountInfos { get; set; }
}
