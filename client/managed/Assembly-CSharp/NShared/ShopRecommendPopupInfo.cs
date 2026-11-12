namespace NShared;

[JsonSerializable(Javascript = true)]
public class ShopRecommendPopupInfo
{
	public string Url { get; set; }

	public int ProductIndex { get; set; }
}
