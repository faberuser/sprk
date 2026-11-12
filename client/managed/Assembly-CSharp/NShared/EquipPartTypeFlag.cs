using System;

namespace NShared;

[Flags]
public enum EquipPartTypeFlag
{
	None = 0,
	Weapon = 1,
	Armor = 2,
	Accessory = 4,
	SecondGear = 8,
	Artifact = 0x10,
	Orb = 0x20,
	Treasure = 0x40,
	__MAX__ = 0x41
}
