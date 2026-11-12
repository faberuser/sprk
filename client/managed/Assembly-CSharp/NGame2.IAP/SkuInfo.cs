using NShared;
using NVespa.NGlobal;

namespace NGame2.IAP;

public class SkuInfo
{
	public string price { get; private set; }

	public string productId { get; private set; }

	public string title { get; private set; }

	public string priceCurrencyCode { get; private set; }

	public bool Purchased { get; private set; }

	public override string ToString()
	{
		return $"productId : {productId} price : {price} priceCurrencyCode : {priceCurrencyCode}";
	}

	public SkuInfo(NewPayShopProductInfo productInfo)
	{
		price = productInfo.USD;
		productId = productInfo.GoogleId;
		title = LocalizationManager.GetStringDirect(productInfo.ProductName);
		priceCurrencyCode = "USD";
		Purchased = false;
		SetReplaceCurrencyInfo(this);
	}

	public static void SetReplaceCurrencyInfo(SkuInfo info)
	{
		_ = info.price;
	}
}
