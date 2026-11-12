namespace NShared;

[JsonSerializable(Javascript = true)]
public class ShopRestockTimeInfo : IAccountAttrKey<int>
{
	int IAccountAttrKey<int>.Key => ShopIndex;

	public int ShopIndex { get; set; }

	public int ShopItemListIndex { get; set; }

	public string RestockTime { get; set; }

	public int RestockCount { get; set; }

	public string RestockCountResetTime { get; set; }
}
