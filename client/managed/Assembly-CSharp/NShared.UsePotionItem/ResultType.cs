namespace NShared.UsePotionItem;

public enum ResultType
{
	Fail,
	Success,
	ItemNotOwned,
	ItemDataNotFound,
	ItemTypeMismatch,
	PotionDataNotFound,
	[Comment("영웅의 초월포인트가 잘못 되었습니다.")]
	CreatureDataNotFound,
	InvalidAction,
	InvalidItemCount,
	InvalidHeroIndex,
	InvalidHeroExp,
	[Comment("영웅의 초월포인트가 이미 최대값입니다.")]
	MaxHeroTranscendPoint,
	[Comment("영웅의 초월포인트가 잘못 되었습니다.")]
	InvalidHeroTranscendPoint,
	HeroNotOwned,
	MaxHeroLevel,
	InvalidActionValue,
	InvalidStaminaType,
	ItemExpired,
	InvalidChapterIndex,
	PlayerDungeonNotFound,
	[Comment("챕터의 게임데이터를 찾을수 없습니다.")]
	NotFoundChapterData,
	[Comment("이전 던전을 클리어하지 않은 상태입니다.")]
	IsNotOpenDungeon,
	[Comment("충전이 불가능한 상태입니다.")]
	NotAvailableRecharge,
	[Comment("초월한 영웅이 아닙니다.")]
	NotHeroTanscended
}
