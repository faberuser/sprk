namespace NShared;

public enum OperationType
{
	None,
	[DamageOrHealOperationType]
	GetHeal,
	[DamageOrHealOperationType(IsPercent = true)]
	GetHealR,
	[DamageOrHealOperationType]
	GetHot,
	[DamageOrHealOperationType(IsPercent = true)]
	GetHotR,
	[DamageOrHealOperationType(IsPercent = true)]
	GetHealByMaxHpR,
	[DamageOrHealOperationType(IsPercent = true)]
	GetHotByMaxHpR,
	[DamageOrHealOperationType]
	GetDamage,
	[DamageOrHealOperationType]
	GetPhysicalDamage,
	[DamageOrHealOperationType]
	GetMagicalDamage,
	[DamageOrHealOperationType(IsPercent = true)]
	GetDamageR,
	[DamageOrHealOperationType(IsPercent = true)]
	GetDamageByMaxHpR,
	[DamageOrHealOperationType(IsPercent = true)]
	GetPhysicalDamageR,
	[DamageOrHealOperationType(IsPercent = true)]
	GetPhysicalDamageByMaxHpR,
	[DamageOrHealOperationType(IsPercent = true)]
	GetMagicalDamageR,
	[DamageOrHealOperationType(IsPercent = true)]
	GetMagicalDamageByMaxHpR,
	[DamageOrHealOperationType(IsPercent = true)]
	GetStateDamageR,
	[DamageOrHealOperationType(IsPercent = true)]
	GetCriticalDamageR,
	[DamageOrHealOperationType]
	GetDot,
	[DamageOrHealOperationType]
	GetPhysicalDot,
	[DamageOrHealOperationType]
	GetMagicalDot,
	[DamageOrHealOperationType(IsPercent = true)]
	GetDotR,
	[DamageOrHealOperationType(IsPercent = true)]
	GetPhysicalDotR,
	[DamageOrHealOperationType(IsPercent = true)]
	GetMagicalDotR,
	[DamageOrHealOperationType(IsPercent = true)]
	GetPhysicalDotByMaxHpR,
	[DamageOrHealOperationType(IsPercent = true)]
	GetMagicalDotByMaxHpR,
	[DamageOrHealOperationType]
	GetMana,
	[DamageOrHealOperationType(IsPercent = true)]
	GetManaR,
	[DamageOrHealOperationType]
	GetMot,
	[DamageOrHealOperationType(IsPercent = true)]
	GetMotR,
	[OperationType]
	ModMaxHp,
	[OperationType(IsPercent = true)]
	ModMaxHpR,
	[OperationType]
	ModMaxMp,
	[OperationType(IsPercent = true)]
	ModMaxMpR,
	[OperationType]
	ModDefence,
	[OperationType]
	ModPhysicalDefence,
	[OperationType]
	ModMagicalDefence,
	[OperationType]
	ModMaxHpDefence,
	[OperationType(IsPercent = true)]
	ModMaxHpDefenceR,
	[OperationType(IsPercent = true)]
	ModDefenceR,
	[OperationType(IsPercent = true)]
	ModPhysicalDefenceR,
	[OperationType(IsPercent = true)]
	ModMagicalDefenceR,
	[OperationType]
	ModPower,
	[OperationType]
	ModPhysicalPower,
	[OperationType]
	ModMagicalPower,
	[OperationType(IsPercent = true)]
	ModPowerR,
	[OperationType(IsPercent = true)]
	ModPhysicalPowerR,
	[OperationType(IsPercent = true)]
	ModMagicalPowerR,
	[OperationType(IsPercent = true)]
	ModHealPower,
	[OperationType(IsPercent = true)]
	ModRecoverPower,
	[OperationType(IsPercent = true)]
	ModBlockChance,
	[OperationType(IsPercent = true)]
	ModPhysicalBlockChance,
	[OperationType(IsPercent = true)]
	ModMagicalBlockChance,
	[OperationType(IsPercent = true)]
	ModBlockPower,
	[OperationType(IsPercent = true)]
	ModPhysicalBlockPower,
	[OperationType(IsPercent = true)]
	ModMagicalBlockPower,
	[OperationType(IsPercent = true)]
	ModComboPowerR,
	[OperationType(IsPercent = true)]
	ModPiercePower,
	[OperationType(IsPercent = true)]
	ModPhysicalPiercePower,
	[OperationType(IsPercent = true)]
	ModMagicalPiercePower,
	[OperationType(IsPercent = true)]
	ModAntiDebuffChance,
	[OperationType(IsPercent = true)]
	ModAntiCcChance,
	[OperationType(IsPercent = true)]
	ModHitCcChance,
	[OperationType(IsPercent = true)]
	ModHitDebuffChance,
	[OperationType(IsPercent = true)]
	ModCriticalPower,
	[OperationType(IsPercent = true)]
	ModPhysicalCriticalPower,
	[OperationType(IsPercent = true)]
	ModMagicalCriticalPower,
	[OperationType(IsPercent = true)]
	ModCriticalChance,
	[OperationType(IsPercent = true)]
	ModPhysicalCriticalChance,
	[OperationType(IsPercent = true)]
	ModMagicalCriticalChance,
	[OperationType(IsPercent = true)]
	ModCriticalDodgeChance,
	[OperationType(IsPercent = true)]
	ModPhysicalCriticalDodgeChance,
	[OperationType(IsPercent = true)]
	ModMagicalCriticalDodgeChance,
	[OperationType(IsPercent = true)]
	ModDodgeChance,
	[OperationType(IsPercent = true)]
	ModPhysicalDodgeChance,
	[OperationType(IsPercent = true)]
	ModMagicalDodgeChance,
	[OperationType(IsPercent = true)]
	ModHitChance,
	[OperationType(IsPercent = true)]
	ModPhysicalHitChance,
	[OperationType(IsPercent = true)]
	ModMagicalHitChance,
	[OperationType]
	ModAllSkillPower,
	[OperationType(IsPercent = true)]
	ModAllSkillPowerR,
	[OperationType(IsPercent = true)]
	ModDotPower,
	[OperationType(IsPercent = true)]
	ModPhysicalDotPower,
	[OperationType(IsPercent = true)]
	ModMagicalDotPower,
	[OperationType(IsPercent = true)]
	ModIgnoreBlockPower,
	[OperationType(IsPercent = true)]
	ModPhysicalIgnoreBlockPower,
	[OperationType(IsPercent = true)]
	ModMagicalIgnoreBlockPower,
	[OperationType(IsPercent = true)]
	ModIgnoreDefencePower,
	[OperationType(IsPercent = true)]
	ModPhysicalIgnoreDefencePower,
	[OperationType(IsPercent = true)]
	ModMagicalIgnoreDefencePower,
	[OperationType(IsPercent = true)]
	ModPowerRByStageLevel,
	[OperationType(IsPercent = true)]
	ModDefenceRByStageLevel,
	[OperationType(IsPercent = true)]
	ModMaxHpRByStageLevel,
	[OperationType(IsPercent = true)]
	ModWeakness,
	[OperationType(IsPercent = true)]
	ModPhysicalWeakness,
	[OperationType(IsPercent = true)]
	ModMagicalWeakness,
	[OperationType]
	ModDebuffTime,
	[OperationType(IsPercent = true)]
	ModDebuffTimeR,
	[OperationType]
	ModCcTime,
	[OperationType(IsPercent = true)]
	ModCcTimeR,
	[OperationType(IsPercent = true)]
	ModCriticalWeakness,
	[OperationType(IsPercent = true)]
	ModPhysicalCriticalWeakness,
	[OperationType(IsPercent = true)]
	ModMagicalCriticalWeakness,
	[OperationType]
	ModCooltime,
	[OperationType(IsPercent = true)]
	ModCooltimeR,
	[OperationType]
	ModAttackSpeed,
	[OperationType(IsPercent = true)]
	ModAttackSpeedR,
	[OperationType(IsPercent = true)]
	ModHpStealPower,
	[OperationType(IsPercent = true)]
	ModToughness,
	[OperationType(IsPercent = true)]
	ModPhysicalToughness,
	[OperationType(IsPercent = true)]
	ModMagicalToughness,
	[OperationType]
	ModHpOnDamage,
	[OperationType]
	ModHpOnAttack,
	[OperationType]
	ModHpOnKill,
	[OperationType]
	ModHpOnWave,
	[OperationType(IsPercent = true)]
	ModHpRatioOnDamage,
	[OperationType(IsPercent = true)]
	ModHpRatioOnAttack,
	[OperationType(IsPercent = true)]
	ModHpRatioOnKill,
	[OperationType(IsPercent = true)]
	ModHpRatioOnWave,
	[OperationType]
	ModMpOnAttack,
	[OperationType(IsPercent = true)]
	ModMpOnAttackR,
	[OperationType]
	ModMpOnDamage,
	[OperationType(IsPercent = true)]
	ModMpOnDamageR,
	[OperationType]
	ModMpOnKill,
	[OperationType(IsPercent = true)]
	ModMpOnKillR,
	[OperationType]
	ModMpOnTime,
	[OperationType(IsPercent = true)]
	ModMpOnTimeR,
	[OperationType]
	ModMpOnWave,
	[OperationType(IsPercent = true)]
	ModMpOnWaveR,
	[OperationType]
	ModMpRegenRatioM,
	[OperationType(IsPercent = true)]
	ModMpRegenRatioMR,
	ModStatByHp,
	[OperationType(IsPercent = true)]
	ModStatByHpR,
	[OperationType(IsPercent = true)]
	ModAttackPower,
	[OperationType(IsPercent = true)]
	ModDefensePower,
	[OperationType(IsPercent = true)]
	ModPhysicalAttackPower,
	[OperationType(IsPercent = true)]
	ModMagicalAttackPower,
	[OperationType]
	SetState,
	[OperationType]
	ImmuneCc,
	[OperationType]
	ImmuneState,
	[OperationType]
	Immortal,
	[OperationType(ApplyTargetFlag = (OperationApplyTargetFlag.Alive | OperationApplyTargetFlag.Dead))]
	Knockback,
	[OperationType(ApplyTargetFlag = (OperationApplyTargetFlag.Alive | OperationApplyTargetFlag.Dead))]
	Pulling,
	[OperationType]
	Stun,
	[OperationType]
	Down,
	[OperationType]
	Shackle,
	[OperationType]
	Silence,
	[OperationType]
	Burn,
	[OperationType]
	Blind,
	[OperationType]
	Banish,
	[OperationType]
	Frozen,
	[OperationType]
	Electronic,
	[OperationType]
	Stone,
	[OperationType]
	Allure,
	[OperationType]
	ForceAutoMode,
	[OperationType]
	Untargetable,
	ModStateTime,
	ModStateTimeR,
	ModStateCount,
	[OperationType]
	Cleanse,
	Dispel,
	ClearSkillOperation,
	ClearState,
	DashToTarget,
	DashFromMyself,
	[OperationType(IsPercent = true)]
	CreateShieldR,
	[OperationType(ApplyTargetFlag = (OperationApplyTargetFlag.Alive | OperationApplyTargetFlag.Dead))]
	CreateProjectile,
	[DamageOrHealOperationType(IsPercent = true)]
	GetChainHealR,
	ModCriticalHealToShieldR,
	EscapeDeath,
	ModAttackAmount,
	ModAttackFlag,
	ModAttackByBoss,
	ModAttackByRace,
	ModDamage,
	ModTriggerStat,
	DodgeDamage,
	FlyOperation,
	[DamageOrHealOperationType(IsPercent = true)]
	GetHealByDamageR,
	[DamageOrHealOperationType(IsPercent = true)]
	GetHotByDamageR,
	SpawnCreature,
	[OperationType(IsPercent = true)]
	ModDamageByState,
	ModNextCooltime,
	ModNextCooltimeR,
	ModActiveCooltime,
	ModActiveCooltimeR,
	ModNextActiveCooltime,
	ModNextActiveCooltimeR,
	ModNextBaseAttackR,
	ModOperation,
	ModSkillValue,
	ModSkillEnabled,
	ModSkillDisabled,
	ModSkillPower,
	ModSkillPowerR,
	ModSkillInt,
	ModSkill,
	SetSkillValue,
	SetSkillPower,
	SetSkillPowerR,
	StartCooltime,
	SetSkillInt,
	SetSkill,
	UseSkill,
	UseSkillOnTick,
	UsePassiveSkill,
	RegisterPassiveSkill,
	RemovePassiveSkill,
	UseSommoneeSkill,
	MoveRandom,
	PullTarget,
	[OperationType(IsPercent = true)]
	ReflectDamage,
	ModMySkill,
	SetMySkill,
	EnablePassiveSkill,
	BeginResting,
	IgnoreDamageByMaxHpR,
	IgnoreDamageUnderHpR,
	RaiseEventTrigger,
	SetDirection,
	OverrideStat,
	OverrideStatFirst,
	SetDamage,
	[OperationType(IsPercent = true)]
	PropagateDamage,
	ChangeStateValue,
	SubSkill,
	SetTriggerTimer,
	FilterTargetByState,
	IgnoreFilterOnCheckOnly,
	ChangeSkillByState,
	SetCreatureData,
	ModCreatureData,
	SuppressSkillCheck,
	DisableSkillUse,
	Die,
	DisableSkillUntilStateCount,
	ModSetRadiusMm,
	DisableSkillSlotToTarget,
	ModSkillLevelAllSlot,
	ModSkillLevelSlot1,
	ModSkillLevelSlot2,
	ModSkillLevelSlot3,
	ModSkillLevelSlot4,
	ModSoulWeaponMaxUseCount,
	[OperationType(IsPercent = true)]
	ModShieldPowerR,
	ModThreatGain,
	[OperationType(IsPercent = true)]
	ModThreatGainR,
	GetThreat,
	Provoke,
	GetDamageNoTrigger,
	[OperationType(IsPercent = true)]
	GetDamageByDamageR,
	[OperationType(IsPercent = true)]
	GetDamageByStackR,
	[OperationType(IsPercent = true)]
	CreateShieldByDamageR,
	[OperationType(IsPercent = true)]
	GetHealByInitialAmount,
	[OperationType(IsPercent = true)]
	AddStateStackByDamageR,
	[OperationType(IsPercent = true)]
	AddStateStackByHealR,
	UseSkillByDamageStackFixed,
	[OperationType(IsPercent = true)]
	OverrideCalcMaxHpR
}
