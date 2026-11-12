namespace NShared;

[JsonSerializable(Javascript = true)]
public enum EquipPartType
{
	None = 0,
	Weapon = 1,
	Armor = 2,
	Accessory = 3,
	SecondGear = 4,
	Artifact = 5,
	Orb = 6,
	Treasure = 7,
	__MAX__ = 8,
	EnhanceStone = 100,
	WeaponArmorSecondGear = 200,
	WeaponArmor = 201,
	WeaponSecondGear = 202,
	ArmorSecondGear = 203
}
