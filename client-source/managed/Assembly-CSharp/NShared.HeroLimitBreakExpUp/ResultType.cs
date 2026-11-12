namespace NShared.HeroLimitBreakExpUp;

public enum ResultType
{
	Fail,
	Success,
	[Comment("영웅이 존재하지 않습니다.")]
	NoHero,
	[Comment("이미 최대 한계돌파 레벨입니다.")]
	AlreadyMaxLevel,
	[Comment("한계돌파 데이터가 존재하지 않습니다.")]
	NoHeroLimitBreakData,
	[Comment("필요경험치가 존재하지 않습니다.")]
	NoReqLocalExp,
	[Comment("경험치 아이템이 선택되지 않았습니다.")]
	NoExpItems,
	[Comment("경험치 아이템이 충분하지 않습니다.")]
	NotEnoughExpItem
}
