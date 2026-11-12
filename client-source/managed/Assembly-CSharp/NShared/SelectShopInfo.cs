namespace NShared;

[JsonSerializable(Javascript = true)]
public class SelectShopInfo : IAccountAttrKey<int>
{
	int IAccountAttrKey<int>.Key => Index;

	public int Index { get; set; }

	public string BeginTime { get; set; }

	public string EndTime { get; set; }

	public string RestockEndTime { get; set; }

	public string CategoryGroup { get; set; }

	public int ResetCount { get; set; }

	public int ShowIndex { get; set; }

	public PayShopProductItemInfo[] ItemInfos { get; set; }

	public PayShopProductEquipItemInfo[] EquipItemInfos { get; set; }

	public NewPayShopProductInfo[] PayShopProductInfos { get; set; }
}
