namespace NShared.BuyHeroStorageSlot;

public enum ResultType
{
	Fail,
	Success,
	ContentsDisabled,
	NoHero,
	HeroDataNotFound,
	NotEnoughGem,
	[Comment("잘못된 장비 프리셋 슬롯입니다.")]
	InvalidHeroStorageKey,
	[Comment("이미 구매한 장비 프리셋 슬롯입니다.")]
	AlreadyHasHeroStorageKey
}
