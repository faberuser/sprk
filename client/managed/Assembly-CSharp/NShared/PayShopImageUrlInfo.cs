namespace NShared;

[JsonSerializable(Javascript = true)]
public class PayShopImageUrlInfo
{
	public int Index { get; set; }

	public string ImageUrl { get; set; }

	public string Image2Url { get; set; }
}
