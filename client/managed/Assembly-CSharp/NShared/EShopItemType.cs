namespace NShared;

[JsonSerializable(Javascript = true)]
public enum EShopItemType
{
	None,
	EquipItem,
	Consumable,
	Growth,
	ETC
}
