namespace NShared.SetHeroStorageSlot;

public enum ResultType
{
	Fail,
	Success,
	ContentsDisabled,
	[Comment("요청을 처리하기에 너무 짧은 시간입니다. 잠시 뒤에 시도해주세요.")]
	HeroStorageSlotTimeTooShort,
	NoHero,
	HeroDataNotFound,
	[Comment("장비 장착 슬롯 테이블 데이터를 찾을 수 없습니다.")]
	EquipSlotDataNotFound,
	[Comment("잘못된 장비 프리셋 슬롯입니다.")]
	InvalidHeroStorageKey
}
