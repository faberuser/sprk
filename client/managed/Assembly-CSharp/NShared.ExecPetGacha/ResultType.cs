namespace NShared.ExecPetGacha;

public enum ResultType
{
	Fail,
	Success,
	GachaDataNotFound,
	[Comment("해당 펫소환이 닫혀있습니다.")]
	ClosedPetGacha,
	[Comment("펫 소환을 할수 없습니다.")]
	CannotPetGacha,
	NotEnoughGold,
	NotEnoughGem,
	InvalidItemCode,
	InvalidGachaRequest,
	InvalidItemIndex,
	InvalidItemType,
	ItemNotOwned,
	InvalidGachaData,
	ContentsDisabled,
	CannotDiscountable,
	NotMatchedDiscountCost,
	AleadyDicounted
}
