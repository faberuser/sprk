namespace NShared.AddHeroStorageSlot;

public enum ResultType
{
	Fail,
	Success,
	ContentsDisabled,
	NoHero,
	HeroDataNotFound,
	[Comment("잘못된 장비 프리셋 슬롯입니다.")]
	InvalidHeroStorageKey,
	[Comment("사용할 수 없는 장비 프리셋 슬롯입니다.")]
	NotOpenedHeroStorageKey
}
