namespace NShared;

public enum ERightMenuButton
{
	None,
	[RightMenuButton(EffectIndex = 0)]
	Hero,
	[RightMenuButton(EffectIndex = 1)]
	Inven,
	[RightMenuButton(EffectIndex = 2)]
	Mission,
	[RightMenuButton(EffectIndex = 3)]
	Achievement,
	[RightMenuButton(EffectIndex = 4)]
	Archive,
	[RightMenuButton(EffectIndex = 5)]
	Friend,
	[RightMenuButton(EffectIndex = 6)]
	PayShop,
	[RightMenuButton(EffectIndex = 7)]
	Portal,
	[RightMenuButton]
	PayShop2,
	[RightMenuButton(EffectIndex = 8)]
	HeroStat,
	ScenarioQuest,
	Attendance5Days,
	PayShopTab,
	FractionInfo,
	Raid,
	TrialTower,
	GuildRaidScoreRank,
	GPGS,
	WorldBoss,
	AttendanceReturn5Days,
	EventWorldBoss,
	PartyPlayDungeon,
	ClassBuff,
	GuildArena,
	SoulWeapon,
	Eclipse,
	GodkingTrial
}
