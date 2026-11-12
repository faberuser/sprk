namespace NShared.EndCampaign;

public enum ResultType
{
	Fail,
	Success,
	ChapterNotFound,
	DungeonNotFound,
	[Comment("전투를 느리게 하셨네요. 좀더 빠른 플레이를 해주세요.")]
	UserCampaignInfoNotFound,
	WrongChapterIndex,
	WrongDungeonIndex,
	NotEnoughStamina,
	InvalidStar,
	DifficultyMismatch,
	MissingBattleType,
	MissingBattleLog,
	MissingCreatureInfo,
	RequestIntegrityCheckError,
	MissingTowerCreatureInfo,
	ParsingTowerCreatureInfoError,
	EmptyTowerEnterHeroes,
	HeroNotFound,
	WorldBossNotFound,
	WorldBossNotActive,
	MissingWorldBossCreatureInfo,
	ParsingWorldBossCreatureInfoError,
	WorldBossHpError,
	WorldBossRankError,
	MultiplayNotEnded,
	ParsingCreatureInfoError,
	NotGuildRaidPlayingAccount,
	InvalidCreatureWaveSlotIndex,
	GuildRaidInfoNotFound,
	AliveEnemyExist,
	AllHeroesDied,
	AlreadyCompletedGuildRaid,
	GuildRaidCanceled,
	ContentsDisabled,
	GuildRaidDungeonDataNotFound,
	[Comment("길드레이드가 개방되어 있지 않습니다.")]
	GuildRaidNotOpened,
	[Comment("길드레이드 초기화가 진행중입니다. 전투가 취소됩니다.")]
	GuildRaidInitializing,
	[Comment("길드레이드 다음 단계 게임데이터를 찾을 수 없습니다.")]
	GuildRaidNextDungeonDataNotFound,
	[Comment("이미 몬스터가 처치되었습니다. 입장권을 소비한 경우 입장권을 반환합니다.")]
	GuildGuildRaidAlreadyCompleted,
	[Comment("이미 모든 단계가 클리어 된 길드 레이드입니다. 입장권을 소비한 경우 입장권을 반환합니다.")]
	GuildGuildRaidAlreadyFinalCompleted,
	[Comment("월드보스에게 대미지를 전현 입히지 못했습니다")]
	WorldBossDamagedByZero,
	WorldBossCanceled,
	WorldBossSeasonChanged,
	StartedWorldBossNotFound,
	WorldBossDataNotFound,
	WithdrawPenalty,
	GuildContentsRestricted,
	ChallengeRaidScoreError,
	ChallengeRaidRankError,
	EventWorldBossCanceled,
	EventWorldBossSeasonChanged,
	EventWorldBossLevelChanged,
	StartedEventWorldBossNotFound,
	EventWorldBossDataNotFound,
	EventWorldBossHpError,
	ChallengeRaidNotOpened,
	ChallengeRaidDataNotFound,
	[Comment("플레이어 정보에서 길드 토벌전 데이터를 찾을 수 없습니다.")]
	GuildSuppressRaidDataNotFound,
	[Comment("진행중인 길드 토벌전이 존재하지 않습니다.")]
	GuildSuppressSessionNotFound,
	[Comment("길드 토벌전이 전투 결과가 잘못 되었습니다.")]
	GuildSuppressBattleResultError,
	[Comment("길드 토벌전 보스가 이미 사망하였습니다.")]
	GuildSuppressAlreadyCompleted,
	[Comment("길드 토벌전 참가정보가 존재하지 않습니다.")]
	GuildSuppressGroupInfoNotFound,
	[Comment("변조된 데이터로 정보를 요청하였습니다.")]
	ModulatedData,
	GodkingTrialDungeonInfoNotFound,
	NotFoundAffixPoint,
	NotFoundAffixMultiple,
	NotFoundClearPoint
}
