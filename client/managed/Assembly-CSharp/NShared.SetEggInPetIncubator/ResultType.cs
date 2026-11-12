namespace NShared.SetEggInPetIncubator;

public enum ResultType
{
	Fail,
	Success,
	NotFoundIncubatorData,
	NotFoundPetEggData,
	NotFoundItemData,
	IsNotEggItem,
	NotOpenSlot,
	[Comment("슬롯에 장착된 부화가기 아닙니다.")]
	IsNotMounting,
	[Comment("보유중인 부화기가 아닙니다.")]
	NotHaveIncubator,
	[Comment("부화기에 이미 알이 세팅 되어 있습니다.")]
	AlreadySetEggExist,
	NotEnoughItem,
	ContentsDisabled
}
