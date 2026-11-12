namespace NShared.BuyCostume;

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
	SoldOut,
	NotEnoughGold,
	NotEnoughGem,
	NotEnoughMileage,
	InvalidPrice,
	EquipItemFull,
	NoShopData,
	ContentsDisabled,
	[Comment("구매 할 수 없는 코스튬입니다.")]
	NotForSale,
	[Comment("서브스토리 게임데이터가 존재하지 않습니다.")]
	SubStoryDataNotFound,
	[Comment("서브스토리를 완료후 구매가 가능합니다.")]
	NeedSubStory
}
