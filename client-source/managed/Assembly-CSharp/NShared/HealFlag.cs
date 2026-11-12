using System;

namespace NShared;

[Flags]
public enum HealFlag
{
	None = 0,
	Critical = 2,
	CriticalHealToShield = 4,
	NoCritical = 8,
	HpSteal = 0x10,
	HpOnAttack = 0x20,
	HpOnKill = 0x40,
	HpOnDamage = 0x80,
	HpOnWave = 0x100,
	HpOnTime = 0x200,
	Supplemently = 0x400,
	NoUiDisplay = 0x800,
	NoTriggerOnZeroHeal = 0x1000,
	DerivedHeal = 0x2000
}
