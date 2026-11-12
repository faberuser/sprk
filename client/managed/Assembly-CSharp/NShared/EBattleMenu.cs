using System;

namespace NShared;

[Flags]
public enum EBattleMenu : ulong
{
	None = 0uL,
	TimeStatus = 1uL,
	SystemButton = 2uL,
	WaveStatus = 4uL,
	LootingStatus = 8uL,
	Retry = 0x10uL,
	WaitingMessage = 0x20uL,
	ExitButton = 0x40uL,
	DamageMeter = 0x80uL,
	HeroParty = 0x100uL,
	Hud = 0x200uL,
	BoosterStatus = 0x400uL,
	QuestStatus = 0x800uL,
	Chat = 0x1000uL,
	Emoti = 0x2000uL,
	GuildRaidScoreStatus = 0x4000uL,
	SystemButtonAuto = 0x8000uL,
	ReplayExitButton = 0x10000uL,
	WorldBossScoreStatus = 0x20000uL,
	SystemButtonAutoSimple = 0x40000uL,
	Leave = 0x80000uL,
	GuildSuppressScorestatus = 0x100000uL,
	PartyExitButton = 0x200000uL,
	ChallengeRaidScorestatus = 0x400000uL,
	EmoticonPopup = 0x800000uL,
	PvpGiveUp = 0x1000000uL,
	ExitSmallButton = 0x2000000uL,
	DeckStatus = 0x4000000uL,
	SwitchCamera = 0x8000000uL,
	DungeonSkill = 0x10000000uL,
	SystemButtonSpeed = 0x20000000uL,
	SystemButtonAutoFastContinue = 0x40000000uL,
	KillMeter = 0x80000000uL,
	ShardGauge = 0x100000000uL,
	TPSDamageMeter = 0x200000000uL,
	CardPick = 0x400000000uL,
	MPSDamageMeter = 0x800000000uL
}
