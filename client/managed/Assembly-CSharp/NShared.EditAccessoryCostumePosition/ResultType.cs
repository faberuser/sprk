namespace NShared.EditAccessoryCostumePosition;

public enum ResultType
{
	Fail,
	Success,
	[Comment("해당 영웅을 보유하고 있지 않습니다.")]
	NotHaveHero,
	[Comment("악세서리 코스튬 게임데이터가 존재하지 않습니다.")]
	AccessoryCostumeDataNotFound,
	[Comment("해당 영웅이 착용 할수 없는 상품입니다.")]
	NotCorrectHero,
	[Comment("보유하지 않은 악세서리 코스튬입니다.")]
	NotHaveAccessoryCostume
}
