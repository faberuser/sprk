namespace NShared;

[JsonSerializable(Javascript = true)]
public enum PayShopPurchaseLogType
{
	None,
	Hero,
	Costume,
	Equip,
	All,
	Pet
}
