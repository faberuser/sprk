namespace NShared;

public enum SkillUseResult
{
	Success,
	Fail,
	BattleDisabled,
	Died,
	AlreadyCasting,
	AlreadyUsing,
	InvalidState,
	Cooltime,
	NotEnoughMp,
	InvalidTarget,
	InvalidFriendlyType,
	InvalidSkillLevel,
	TargetTooFar,
	TargetTooNear,
	ConditionFail,
	CreatureTagTypeMismatch,
	UseSkillMaxCount,
	NotEnoughStack,
	NotEnoughStateCount,
	EnhancedSuccess,
	DisableSkillSlot
}
