namespace NShared.EndContent;

public enum ResultType
{
	Fail,
	Success,
	ContentsDisabled,
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
	HeroNotFound,
	[Comment("변조된 데이터로 정보를 요청하였습니다.")]
	ModulatedData,
	NotEnoughItem,
	WrongFlaskIndex,
	WrongInitialPoint,
	CannotPlayDungeon,
	NotOpenedDungeon,
	RewardDataNotFound,
	InvalidData,
	WrongGroupIndex,
	WrongDungeonType,
	ClosedDungeon
}
