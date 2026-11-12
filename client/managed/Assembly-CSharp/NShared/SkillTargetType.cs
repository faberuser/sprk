namespace NShared;

public enum SkillTargetType
{
	[SkillTarget(IsFriendly = true)]
	Myself,
	Trigger,
	TriggerTarget,
	[SkillTarget(CheckDeadTarget = true)]
	PrevUsedTarget,
	[SkillTarget(CheckDeadTarget = true)]
	PrevFiredTarget,
	Ally,
	Enemy,
	[SkillTarget(ExcludeMe = true)]
	Friend,
	MinHpAlly,
	MinHpEnemy,
	[SkillTarget(ExcludeMe = true)]
	MinHpFriend,
	[SkillTarget(IsRandom = true)]
	RandomAlly,
	[SkillTarget(IsRandom = true)]
	RandomEnemy,
	[SkillTarget(IsRandom = true, ExcludeMe = true)]
	RandomFriend,
	FarAlly,
	FarEnemy,
	[SkillTarget(ExcludeMe = true)]
	FarFriend,
	MaxPhysicalPowerAlly,
	MaxPhysicalPowerEnemy,
	[SkillTarget(ExcludeMe = true)]
	MaxPhysicalPowerFriend,
	MaxMagicalPowerAlly,
	MaxMagicalPowerEnemy,
	[SkillTarget(ExcludeMe = true)]
	MaxMagicalPowerFriend,
	MaxPhysicalDefenseAlly,
	MaxPhysicalDefenseEnemy,
	[SkillTarget(ExcludeMe = true)]
	MaxPhysicalDefenseFriend,
	MaxMagicalDefenseAlly,
	MaxMagicalDefenseEnemy,
	[SkillTarget(ExcludeMe = true)]
	MaxMagicalDefenseFriend,
	MinPhysicalDefenseAlly,
	MinPhysicalDefenseEnemy,
	[SkillTarget(ExcludeMe = true)]
	MinPhysicalDefenseFriend,
	MinMagicalDefenseAlly,
	MinMagicalDefenseEnemy,
	[SkillTarget(ExcludeMe = true)]
	MinMagicalDefenseFriend,
	[SkillTarget(IsFriendly = true)]
	Summoner,
	[SkillTarget(IsFriendly = true)]
	Summonee,
	SummonerTarget,
	SummoneeTarget,
	Class,
	HighestTotalDamageAlly,
	HighestTotalDamageEnemy,
	[SkillTarget(ExcludeMe = true)]
	HighestTotalDamageFriend,
	LowestTotalDamageAlly,
	LowestTotalDamageEnemy,
	[SkillTarget(ExcludeMe = true)]
	LowestTotalDamageFriend,
	HighestThreatener,
	LowestThreatener,
	[SkillTarget(IsFriendly = true)]
	HighestThreatenerAlly,
	[SkillTarget(IsFriendly = true)]
	LowestThreatenerAlly
}
