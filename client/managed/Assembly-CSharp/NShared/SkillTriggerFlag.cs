using System;

namespace NShared;

[Flags]
public enum SkillTriggerFlag
{
	None = 0,
	CancelUsingSkill = 1,
	CanUseWhenDied = 2,
	NoActiveCoolTime = 4,
	UseRelativeTimer = 8,
	CanUseOnWaveEnd = 0x10,
	ForceImmune = 0x20,
	CanNotCancelBySlotSkill = 0x40,
	NoNextActiveSkillCheck = 0x80,
	UseNextSkillOnCancel = 0x100,
	HideChangedSkillInfo = 0x200,
	UpdateSkillCondition = 0x400,
	ExcuteDamageTriggerPrevOnKill = 0x800,
	FireNextSkillOnReserve = 0x1000,
	CanUseWhenDisabledSkillSlot = 0x2000
}
