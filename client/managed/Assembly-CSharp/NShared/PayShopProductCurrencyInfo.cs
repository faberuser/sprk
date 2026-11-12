namespace NShared;

[JsonSerializable(Javascript = true)]
public class PayShopProductCurrencyInfo
{
	public CurrencyType CurrencyType { get; set; }

	public int Amount { get; set; }
}
