namespace NShared.UseAccessorySelectItem;

public enum ResultType
{
	Fail,
	Success,
	[Comment("요청에 필요한 필수 값이 존재하지 않습니다.")]
	RequiredValueEmpty,
	[Comment("소유하고 있지 않은 아이템 입니다.")]
	ItemNotOwned,
	[Comment("아이템 게임데이터가 존재하지 않습니다.")]
	ItemDataNotFound,
	[Comment("아이템 악세서리 코스튬 게임데이터가 존재하지 않습니다.")]
	AccessoryCostumeDataNotFound,
	[Comment("아이템 악세서리 선택 아이템 게임데이터가 존재하지 않습니다.")]
	AccessorySelectDataNotFound,
	[Comment("선택이 불가능한 영웅입니다.")]
	NotSelectableHero,
	[Comment("구매가 불가능한 악세서리 코스튬 입니다.")]
	CostumeNotOpened,
	[Comment("해당 영웅을 보유하고 있지 않습니다.")]
	NotHaveHero,
	[Comment("해당 영웅이 착용 할수 없는 상품입니다.")]
	NotCorrectHero,
	[Comment("이미 보유한 악세서리 코스튬입니다.")]
	AlreadyHaveAccessoryCostume,
	[Comment("악세서리 코스튬을 가지고 있지 않습니다.")]
	AccessoryCostumeNotOwned,
	[Comment("대체 보상의 게임데이터가 존재하지 않습니다.")]
	AllOwnedRewardIndexNotFound
}
