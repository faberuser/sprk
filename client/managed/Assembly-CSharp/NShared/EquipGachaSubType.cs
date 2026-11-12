namespace NShared;

[JsonSerializable(Javascript = true)]
public enum EquipGachaSubType
{
	None,
	All,
	Weapon,
	Treasure,
	AwakenWeapon,
	AwakenTreasure,
	Starter
}
