namespace NShared.ResurrectionHero;

public enum ResultType
{
	Fail,
	Success,
	HeroNotFound,
	CannotResurrectionHero,
	InvalidCost,
	NotEnoughGem,
	[Comment("시련의 전장 - 시즌정보를 찾을 수 없습니다.")]
	NotFoundSeasonData,
	InternalServerError,
	ContentsDisabled
}
