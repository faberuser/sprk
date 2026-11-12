namespace NShared.BuyCustomizingCostumes;

public enum ResultType
{
	Fail,
	Success,
	[Comment("요청에 필요한 필수 값이 존재하지 않습니다.")]
	RequiredValueEmpty,
	[Comment("해당 영웅을 보유하고 있지 않습니다.")]
	NotHaveHero,
	[Comment("코스튬 게임데이터가 존재하지 않습니다.")]
	CostumeDataNotFound,
	[Comment("무기 코스튬 게임데이터가 존재하지 않습니다.")]
	WeaponCostumeDataNotFound,
	[Comment("헤어 코스튬 게임데이터가 존재하지 않습니다.")]
	HairCostumeDataNotFound,
	[Comment("코스튬 게임데이터 영응 등급 정보가 존재하지 않습니다.")]
	StarValueNotFound,
	[Comment("해당 코스튬의 기본 코스튬을 가지고 있지 않습니다.")]
	NotHaveNeedCostume,
	[Comment("구매가 불가능한 코스튬 입니다.")]
	CostumeNotOpened,
	[Comment("구매가 불가능한 기본 지급 코스튬 입니다.")]
	CostumeIsDefault,
	[Comment("서브스토리 게임데이터가 존재하지 않습니다.")]
	SubStoryDataNotFound,
	[Comment("서브스토리를 완료후 구매가 가능합니다.")]
	NeedSubStory,
	[Comment("이미 보유한 악세서리 코스튬입니다.")]
	AlreadyHaveAccessoryCostume,
	[Comment("악세서리 코스튬 게임데이터가 존재하지 않습니다.")]
	AccessoryCostumeDataNotFound,
	[Comment("구매가 불가능한 악세서리 코스튬 입니다.")]
	AccessoryCostumeNotOpened,
	[Comment("판매용 상품이 아닙니다.")]
	NotForSale,
	[Comment("해당 영웅이 착용 할수 없는 상품입니다.")]
	NotCorrectHero,
	[Comment("중복된 부위의 코스튬을 구매 착용 할수 없습니다.")]
	DuplicatePartType,
	[Comment("악세서리 코스튬 해제에 실패하였습니다.")]
	UnsetAccessoryCostumeFail,
	[Comment("보유하지 않은 무기 코스튬입니다.")]
	NotHaveWeaponCostume,
	[Comment("구매 가격정보가 잘못되었습니다.")]
	InvalidPrice,
	[Comment("구매에 필요한 루비가 부족합니다.")]
	NotEnoughGem,
	[Comment("구매에 필요한 골드가 부족합니다.")]
	NotEnoughGold,
	[Comment("보유하지 않은 코스튬입니다.")]
	CostumeNotOwned,
	[Comment("함께 착용 할 수 없는 조합입니다.")]
	CannotBeWornTogether,
	[Comment("단독 착용/구매 할 수 없습니다.")]
	CannotBeAlone
}
