namespace NShared;

[JsonSerializable(Javascript = true)]
public class PayShopProductItemInfo
{
	public int ItemIndex { get; set; }

	public int ItemCount { get; set; }
}
