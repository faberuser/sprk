namespace NShared.PetAwaken;

public enum ResultType
{
	Fail,
	Success,
	StatDataNotFound,
	[Comment("보유하지 않은 펫입니다.")]
	NotHavePet,
	[Comment("각성 불가능한 펫입니다.")]
	CannotAwakenPet,
	[Comment("최대 각성상태입니다.")]
	CannotAwakenMore,
	[Comment("잘못된 가격정보입니다.")]
	MaterialItemPriceError,
	NotEnoughGold,
	NotEnoughItem,
	InvalidItemData
}
