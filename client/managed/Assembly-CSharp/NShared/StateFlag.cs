using System;

namespace NShared;

[Flags]
public enum StateFlag
{
	None = 0,
	CanMove = 1,
	CanPhysicalSkill = 2,
	CanMagicalSkill = 4,
	CanBaseAttack = 8,
	CanBeTargetedByAlly = 0x10,
	CanBeTargetedByEnemy = 0x20,
	CanBePhysicalCc = 0x40,
	CanBeMagicalCc = 0x80,
	CanBePhysicalDamaged = 0x100,
	CanBeMagicalDamaged = 0x200,
	CanHitPhysicalAttack = 0x400,
	CanHitMagicalAttack = 0x800,
	CanPhysicalBlockAll = 0x1000,
	CanMagicalBlockAll = 0x2000,
	CanPhysicalDodgeAll = 0x4000,
	CanMagicalDodgeAll = 0x8000,
	IsSticky = 0x10000,
	IsPositive = 0x20000,
	IsNegative = 0x40000,
	IsCc = 0x80000,
	MaxStack = 0x100000,
	CanPassiveSkill = 0x200000,
	RejectReservedSkill = 0x400000,
	CanUpdateCooltime = 0x800000
}
