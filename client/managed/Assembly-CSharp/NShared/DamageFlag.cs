using System;

namespace NShared;

[Flags]
public enum DamageFlag
{
	None = 0,
	Physical = 1,
	Magical = 2,
	Critical = 4,
	NoCritical = 8,
	Unavoidable = 0x10,
	Blocked = 0x20,
	Dodged = 0x40,
	Dot = 0x80,
	IgnoreDefence = 0x100,
	Immune = 0x200,
	Reflected = 0x400,
	StateAction = 0x800,
	Modified = 0x1000,
	IgnoreImmune = 0x2000,
	IgnoreBlock = 0x4000,
	IgnoreToughness = 0x8000,
	IgnoreWeakness = 0x10000,
	IgnoreDefensePower = 0x20000,
	IgnoreModAttackByRace = 0x40000,
	IgnoreModAttackByBoss = 0x80000,
	IgnoreAttackersTrigger = 0x100000,
	DerivedDamage = 0x200000
}
