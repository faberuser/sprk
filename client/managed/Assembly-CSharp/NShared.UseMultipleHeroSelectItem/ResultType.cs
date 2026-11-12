namespace NShared.UseMultipleHeroSelectItem;

public enum ResultType
{
	Fail,
	Success,
	ItemNotOwned,
	ItemDataNotFound,
	ItemTypeMismatch,
	HeroSelectDataNotFound,
	HeroNotSelectable,
	HeroFragmentDataError,
	HeroDataNotFound,
	AwakeChallengeDataError,
	AwakeItemDataError,
	CreatureStarDataNotFound,
	[Comment("모든 코스튬을 보유하지 않습니다.")]
	AllHeroNotOwned
}
