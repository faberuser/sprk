namespace NShared.UseCostumeSelectItem;

public enum ResultType
{
	Fail,
	Success,
	NoHero,
	HeroDataNotFound,
	AlreadyHaveCostume,
	CostumeDataNotFound,
	CostumeNotOpened,
	CostumeIsDefault,
	NotCorrectHero,
	ItemNotOwned,
	ItemDataNotFound,
	ItemTypeMismatch,
	NotFoundCostumeSelectData,
	[Comment("선택 할 수 없는 코스튬입니다.")]
	CanNotSelectedCostume,
	[Comment("모든 코스튬을 보유하지 않습니다.")]
	AllCostumeNotOwned
}
