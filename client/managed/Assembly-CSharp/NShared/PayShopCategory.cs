namespace NShared;

[JsonSerializable(Javascript = true)]
public enum PayShopCategory
{
	None,
	Special,
	Goods,
	Hero,
	Costume,
	Accessory,
	Equip,
	SpecialResource,
	HouseObjectSkin,
	PayShopEvent
}
