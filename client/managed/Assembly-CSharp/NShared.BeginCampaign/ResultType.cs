namespace NShared.BeginCampaign;

public enum ResultType
{
	Fail,
	Success,
	EquipItemFull,
	ChapterNotFound,
	DungeonNotFound,
	LinkedDungeonNotFound,
	AlreadyCompleted,
	DungeonTypeError,
	CannotVisitDungeon,
	EmptyHeroIndices,
	CannotPlayDungeon,
	NotEnoughStamina,
	NotEnoughGem,
	NotEnoughChallengeChance,
	NotEnoughDungeonKey,
	InvalidTrialHero,
	InvalidDiff,
	NotOpenedDungeon,
	WorldMapEventNotFound,
	EventDungeonIndexMismatch,
	NotEnoughMultiplayMembers,
	MaxDailyCount,
	CheckingIntegrity,
	BattleServerNotFound,
	MissingTowerIndex,
	TowerDataNotFound,
	TowerFloorDataNotFound,
	NoMaxStarHero,
	CannotEnterTowerFloor,
	TowerChapterMismatch,
	TowerDungeonMismatch,
	TowerTeamLevelLimit,
	TowerReqDungeonNotCompleted,
	TowerReqChapterNotCompleted,
	NotEnoughTowerCost,
	TowerNpcNotFound,
	TowerNpcSlotNotFound,
	TowerNpcInfoNotFound,
	DiedHeroCache,
	WrongTowerRewardNo,
	WrongDifficulty,
	NotEnoughPlayKey,
	NotRaidType,
	MissingRaidIndex,
	RaidDataNotFound,
	RaidNotStarted,
	RaidNotProgressing,
	RaidMustMultiplay,
	RaidChapterMismatch,
	RaidDungeonMismatch,
	RaidLevelMismatch,
	PartyDungeonLevelMismatch,
	InvalidRank,
	ContentsDisabled,
	GuildRaidRoomNotFound,
	NotGuildRaidRoomMember,
	InvalidGuldRaidMemberRank,
	NotEnoughEnterGuildRaidCount,
	MissingWorldBossIndex,
	WorldBossNotExist,
	WorldBossNotActive,
	WorldBossDailyRewarding,
	DuplicatedHero,
	HeroAwakeDataNotFound,
	HeroTrialAlreadyCompleted,
	GuildRaidChpaterDataNotFound,
	GuildRaidDungeonDataNotFound,
	[Comment("길드레이드 초기화가 진행중입니다. 전투가 취소됩니다.")]
	GuildRaidInitializing,
	NoGuild,
	GuildRaidInfoNotFound,
	InvalidGuildRaidOpenDuration,
	PlayingGuildRaidExist,
	[Comment("길드레이드가 개방되어 있지 않습니다.")]
	GuildRaidNotOpened,
	NotCurrentGuildRaidDungeon,
	AlreadyCompletedGuildRaid,
	EventDungeonDataNotFound,
	EventDungeonClosed,
	WithdrawPenalty,
	GuildContentsRestricted,
	WorldBossDataNotFound,
	WrongWorldBossChapter,
	WrongWorldBossDungeon,
	ChallengeRaidDataNotFound,
	ChallengeRaidNotOpened,
	NotAvailableHero,
	MissingEventWorldBossIndex,
	EventWorldBossNotExist,
	EventWorldBossNotActive,
	EventWorldBossNotPlayableTime,
	EventWorldBossDataNotFound,
	WrongEventWorldBossChapter,
	WrongEventWorldBossDungeon,
	NotEnoughHideoutDungeonCount,
	[Comment("필수 챕터를 클리어하지 않았습니다.")]
	NotCompletedReqDungeon,
	[Comment("진행중인 길드 토벌전이 존재하지 않습니다.")]
	GuildSuppressSessionNotFound,
	[Comment("길드 토벌전이 전투가능 상태가 아닙니다.")]
	GuildSuppressNotBattleState,
	[Comment("길드 토벌전 세션테이블 데이터를 찾을 수 없습니다.")]
	GuildSuppressSessionDataNotFound,
	[Comment("길드 토벌전 참여정보를 찾을 수 없습니다.")]
	GuildSuppressPlayInfoNotFound,
	[Comment("길드 토벌전에 참가 신청되지 않은 상태입니다.")]
	GuildSuppressNotApplied,
	[Comment("이미 완료된 길드 토벌전입니다.")]
	GuildSuppressAlreadyCompleted,
	[Comment("길드 토벌전 던전테이블 데이터를 찾을 수 없습니다.")]
	GuildSuppressDungeonDataNotFound,
	[Comment("이벤트 진행 조건이 없습니다.")]
	MissingBonusEventCondition,
	[Comment("정상적인 길드원이 아니기 때문에 진행에 실패했습니다.")]
	IncorrectGuildMember,
	[Comment("토벌전투 열쇠가 부족합니다.")]
	NotEnoughConquestDungeonCount,
	[Comment("이미 파견중인 영웅입니다.")]
	AlreadyOnBattleHero,
	[Comment("영웅을 찾을수 없습니다.")]
	HeroNotFound,
	[Comment("잘못된 신왕의 시련 챕터 번호입니다.")]
	InvalidGodkingTrialChapterIndex,
	[Comment("신왕의 시련 그룹 데이터를 찾을 수 없습니다.")]
	GodkingTrialGroupDataNotFound,
	[Comment("신왕의 시련 데이터를 찾을 수 없습니다.")]
	GodkingTrialDataNotFound,
	[Comment("전리품 부스터를 사용할 수 없습니다.")]
	CannotUseItemBooster,
	[Comment("신왕의 시련 던전을 찾을 수 없습니다.")]
	GodkingTrialDungeonNotFound,
	[Comment("신왕의 시련 던전이 오픈되지 않았습니다.")]
	GodkingTrialDungeonNotOpened,
	[Comment("이클립스 입장 티켓이 부족합니다.")]
	NotEnoughEclipseKey,
	[Comment("이클립스 던전에 입장이 실패하였습니다.")]
	FailEnterEclipse,
	[Comment("이클립스 던전에 입장 가능 시간이 지났습니다.")]
	EclipseBattleTimeOver,
	[Comment("일일 입장 제한 횟수를 초과하여 입장 할 수 없습니다.")]
	ExceededDailyEntryCount,
	[Comment("지하 통합던전 정보를 찾을 수 없습니다.")]
	NotFoundUnderPrisonInfo,
	[Comment("멀티플레이 컨텐츠가 아닙니다.")]
	IsNotMultiContents,
	[Comment("샤크메 보스던전 입장에 실패하였습니다.")]
	FailEnterShakmehBossDungeon,
	[Comment("샤크메 보스던전에 입장 조건에 문제가 있습니다.")]
	InvalidConditionShakmehBossDungeon,
	[Comment("이미 처치된 몬스터입니다.")]
	NoticeAlreadyDeathMonsterDesc,
	[Comment("마도 인챈트 던전 티켓 보유량이 부족합니다.")]
	NotEnoughTechnoEnchantKey,
	[Comment("참여 영웅 정보와 속성별 영웅 정보가 맞지 않습니다.")]
	NotMatchHeroIndices,
	[Comment("던전이 초기화 되었습니다. 입장하기 위해 오픈 게이지가 필요합니다.")]
	TreasurehouseInitializeEnterPopup,
	[Comment("유효한 패널티/어드벤티지 값이 아닙니다.")]
	NotValidAffixIndex,
	[Comment("허용된 포인트 값을 벗어났습니다.")]
	NotAllowedPoint
}
