namespace NShared;

public enum SkillDisablerType
{
	None,
	ManualUse,
	[SkillDisablerType(DisableAutoUse = true)]
	AutoUse,
	StateCount,
	[SkillDisablerType(DisableAutoUse = true)]
	DisableSkillSlot
}
