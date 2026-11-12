namespace NShared;

[JsonSerializable(Javascript = true)]
public enum StatType
{
	None,
	[StatType(Flag = StatAttributeFlag.Positive)]
	PhysicalAttack,
	[StatType(Flag = (StatAttributeFlag.Positive | StatAttributeFlag.Percent))]
	AddPhysicalAttackR,
	[StatType(Flag = StatAttributeFlag.Positive)]
	PhysicalDefense,
	[StatType(Flag = (StatAttributeFlag.Positive | StatAttributeFlag.Percent))]
	AddPhysicalDefenseR,
	[StatType(Flag = (StatAttributeFlag.Positive | StatAttributeFlag.Percent))]
	PhysicalCriticalChance,
	[StatType(Flag = (StatAttributeFlag.Positive | StatAttributeFlag.Percent))]
	PhysicalCriticalPower,
	[StatType(Flag = (StatAttributeFlag.Negative | StatAttributeFlag.Percent))]
	PhysicalCriticalWeakness,
	[StatType(Flag = (StatAttributeFlag.Positive | StatAttributeFlag.Percent))]
	PhysicalDodgeChance,
	[StatType(Flag = (StatAttributeFlag.Positive | StatAttributeFlag.Percent))]
	PhysicalBlockChance,
	[StatType(Flag = (StatAttributeFlag.Positive | StatAttributeFlag.Percent | StatAttributeFlag.AllowNegative))]
	PhysicalBlockPower,
	[StatType(Flag = (StatAttributeFlag.Positive | StatAttributeFlag.Percent | StatAttributeFlag.AllowNegative))]
	PhysicalHitChance,
	[StatType(Flag = (StatAttributeFlag.Positive | StatAttributeFlag.Percent))]
	PhysicalPiercePower,
	[StatType(Flag = (StatAttributeFlag.Positive | StatAttributeFlag.Percent))]
	PhysicalToughness,
	[StatType(Flag = (StatAttributeFlag.Negative | StatAttributeFlag.Percent))]
	PhysicalWeakness,
	[StatType(Flag = (StatAttributeFlag.Positive | StatAttributeFlag.Percent))]
	PhysicalCriticalDodgeChance,
	[StatType(Flag = (StatAttributeFlag.Positive | StatAttributeFlag.Percent | StatAttributeFlag.AllowNegative))]
	PhysicalAttackPower,
	[StatType(Flag = (StatAttributeFlag.Positive | StatAttributeFlag.Percent | StatAttributeFlag.AllowNegative))]
	PhysicalDefensePower,
	[StatType(Flag = (StatAttributeFlag.Positive | StatAttributeFlag.Percent | StatAttributeFlag.AllowNegative))]
	PhysicalIgnoreDefensePower,
	[StatType(Flag = (StatAttributeFlag.Positive | StatAttributeFlag.Percent | StatAttributeFlag.AllowNegative))]
	PhysicalDotPower,
	[StatType(Flag = (StatAttributeFlag.Positive | StatAttributeFlag.Percent | StatAttributeFlag.AllowNegative))]
	PhysicalIgnoreBlockPower,
	[StatType(Flag = StatAttributeFlag.Positive)]
	MagicalAttack,
	[StatType(Flag = (StatAttributeFlag.Positive | StatAttributeFlag.Percent))]
	AddMagicalAttackR,
	[StatType(Flag = StatAttributeFlag.Positive)]
	MagicalDefense,
	[StatType(Flag = (StatAttributeFlag.Positive | StatAttributeFlag.Percent))]
	AddMagicalDefenseR,
	[StatType(Flag = (StatAttributeFlag.Positive | StatAttributeFlag.Percent))]
	MagicalCriticalChance,
	[StatType(Flag = (StatAttributeFlag.Positive | StatAttributeFlag.Percent))]
	MagicalCriticalPower,
	[StatType(Flag = (StatAttributeFlag.Negative | StatAttributeFlag.Percent))]
	MagicalCriticalWeakness,
	[StatType(Flag = (StatAttributeFlag.Positive | StatAttributeFlag.Percent))]
	MagicalDodgeChance,
	[StatType(Flag = (StatAttributeFlag.Positive | StatAttributeFlag.Percent))]
	MagicalBlockChance,
	[StatType(Flag = (StatAttributeFlag.Positive | StatAttributeFlag.Percent | StatAttributeFlag.AllowNegative))]
	MagicalBlockPower,
	[StatType(Flag = (StatAttributeFlag.Positive | StatAttributeFlag.Percent | StatAttributeFlag.AllowNegative))]
	MagicalHitChance,
	[StatType(Flag = (StatAttributeFlag.Positive | StatAttributeFlag.Percent))]
	MagicalPiercePower,
	[StatType(Flag = (StatAttributeFlag.Positive | StatAttributeFlag.Percent))]
	MagicalToughness,
	[StatType(Flag = (StatAttributeFlag.Negative | StatAttributeFlag.Percent))]
	MagicalWeakness,
	[StatType(Flag = (StatAttributeFlag.Positive | StatAttributeFlag.Percent))]
	MagicalCriticalDodgeChance,
	[StatType(Flag = (StatAttributeFlag.Positive | StatAttributeFlag.Percent | StatAttributeFlag.AllowNegative))]
	HealPower,
	[StatType(Flag = (StatAttributeFlag.Positive | StatAttributeFlag.Percent | StatAttributeFlag.AllowNegative))]
	RecoverPower,
	[StatType(Flag = (StatAttributeFlag.Positive | StatAttributeFlag.Percent | StatAttributeFlag.AllowNegative))]
	MagicalAttackPower,
	[StatType(Flag = (StatAttributeFlag.Positive | StatAttributeFlag.Percent | StatAttributeFlag.AllowNegative))]
	MagicalDefensePower,
	[StatType(Flag = (StatAttributeFlag.Positive | StatAttributeFlag.Percent | StatAttributeFlag.AllowNegative))]
	MagicalIgnoreDefensePower,
	[StatType(Flag = (StatAttributeFlag.Positive | StatAttributeFlag.Percent | StatAttributeFlag.AllowNegative))]
	MagicalDotPower,
	[StatType(Flag = (StatAttributeFlag.Positive | StatAttributeFlag.Percent | StatAttributeFlag.AllowNegative))]
	MagicalIgnoreBlockPower,
	[StatType(Flag = (StatAttributeFlag.Positive | StatAttributeFlag.Percent))]
	AntiCcChance,
	[StatType(Flag = (StatAttributeFlag.Positive | StatAttributeFlag.Percent))]
	AntiDebuffChance,
	[StatType(Flag = StatAttributeFlag.Negative)]
	DebuffTime,
	[StatType(Flag = StatAttributeFlag.Negative)]
	CcTime,
	[StatType(Flag = StatAttributeFlag.Negative)]
	Cooltime,
	[StatType(Flag = (StatAttributeFlag.Positive | StatAttributeFlag.Percent))]
	HitCcChance,
	[StatType(Flag = (StatAttributeFlag.Positive | StatAttributeFlag.Percent))]
	HitDebuffChance,
	[StatType(Flag = StatAttributeFlag.Positive)]
	MaxHp,
	[StatType(Flag = (StatAttributeFlag.Positive | StatAttributeFlag.Percent))]
	AddMaxHpR,
	[StatType(Flag = (StatAttributeFlag.Positive | StatAttributeFlag.Percent))]
	HpStealPower,
	[StatType(Flag = StatAttributeFlag.Positive)]
	HpOnDamage,
	[StatType(Flag = StatAttributeFlag.Positive)]
	HpOnAttack,
	[StatType(Flag = StatAttributeFlag.Positive)]
	HpOnKill,
	[StatType(Flag = StatAttributeFlag.Positive)]
	HpOnWave,
	[StatType(Flag = StatAttributeFlag.Positive)]
	HpOnTime,
	[StatType(Flag = (StatAttributeFlag.Positive | StatAttributeFlag.Percent))]
	HpRatioOnDamage,
	[StatType(Flag = (StatAttributeFlag.Positive | StatAttributeFlag.Percent))]
	HpRatioOnAttack,
	[StatType(Flag = (StatAttributeFlag.Positive | StatAttributeFlag.Percent))]
	HpRatioOnKill,
	[StatType(Flag = (StatAttributeFlag.Positive | StatAttributeFlag.Percent))]
	HpRatioOnWave,
	[StatType(Flag = (StatAttributeFlag.Positive | StatAttributeFlag.Percent))]
	HpRatioOnTime,
	[StatType(Flag = StatAttributeFlag.Positive)]
	MaxMp,
	[StatType(Flag = StatAttributeFlag.Positive)]
	MpOnDamage,
	[StatType(Flag = StatAttributeFlag.Positive)]
	MpOnAttack,
	[StatType(Flag = StatAttributeFlag.Positive)]
	MpOnKill,
	[StatType(Flag = StatAttributeFlag.Positive)]
	MpOnWave,
	[StatType(Flag = StatAttributeFlag.Positive)]
	MpOnTime,
	[StatType(Flag = (StatAttributeFlag.Positive | StatAttributeFlag.Percent))]
	MpRatioOnDamage,
	[StatType(Flag = (StatAttributeFlag.Positive | StatAttributeFlag.Percent))]
	MpRatioOnAttack,
	[StatType(Flag = (StatAttributeFlag.Positive | StatAttributeFlag.Percent))]
	MpRatioOnTime,
	[StatType(Flag = (StatAttributeFlag.Positive | StatAttributeFlag.Percent))]
	AddMpOnDamageR,
	[StatType(Flag = (StatAttributeFlag.Positive | StatAttributeFlag.Percent))]
	AddMpOnAttackR,
	[StatType(Flag = (StatAttributeFlag.Positive | StatAttributeFlag.Percent))]
	AddMpOnTimeR,
	[StatType(Flag = (StatAttributeFlag.Positive | StatAttributeFlag.Percent))]
	MpRegenRatioM,
	[StatType(Flag = (StatAttributeFlag.Positive | StatAttributeFlag.Percent))]
	AttackSpeed,
	[StatType(Flag = StatAttributeFlag.Positive)]
	MoveSpeedMms,
	[StatType(Flag = (StatAttributeFlag.Positive | StatAttributeFlag.Percent))]
	SkillPower,
	[StatType(Flag = (StatAttributeFlag.Negative | StatAttributeFlag.Percent))]
	SkillWeakness,
	[StatType(Flag = (StatAttributeFlag.Positive | StatAttributeFlag.Percent))]
	ComboPower,
	[StatType(Flag = (StatAttributeFlag.Positive | StatAttributeFlag.Percent))]
	CriticalHealToShield,
	[StatType(Flag = StatAttributeFlag.Positive)]
	SkillLevelAllSlot,
	[StatType(Flag = StatAttributeFlag.Positive)]
	SkillLevelSlot1,
	[StatType(Flag = StatAttributeFlag.Positive)]
	SkillLevelSlot2,
	[StatType(Flag = StatAttributeFlag.Positive)]
	SkillLevelSlot3,
	[StatType(Flag = StatAttributeFlag.Positive)]
	SkillLevelSlot4,
	[StatType(Flag = StatAttributeFlag.Positive)]
	SoulWeaponMaxUseCount,
	[StatType(Flag = (StatAttributeFlag.Positive | StatAttributeFlag.Percent))]
	ShieldPowerR,
	Hp,
	HpRatioM,
	Mp,
	MpRatioM,
	AllyCount,
	EnemyCount,
	TargetableEnemyCount,
	BossCcCurGauge,
	BossCcCurGaugeR,
	[StatType(Flag = StatAttributeFlag.Negative)]
	ThreatGain,
	[StatType(Flag = (StatAttributeFlag.Negative | StatAttributeFlag.Percent))]
	ThreatGainR
}
