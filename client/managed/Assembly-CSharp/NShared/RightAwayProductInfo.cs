namespace NShared;

[JsonSerializable(Javascript = true)]
public class RightAwayProductInfo : IAccountAttrKey<int>
{
	int IAccountAttrKey<int>.Key => Index;

	public int Index { get; set; }

	public string EndTime { get; set; }

	public int LimitedQuantity { get; set; }

	public string BannerPath { get; set; }

	public int ShowIndex { get; set; }
}
