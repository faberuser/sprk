namespace NShared;

[JsonSerializable(Javascript = true)]
public class PayShopProductEquipItemInfo
{
	public int ItemIndex { get; set; }

	public int Star { get; set; }

	public int Level { get; set; }
}
