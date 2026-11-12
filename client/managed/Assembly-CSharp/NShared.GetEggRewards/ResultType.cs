namespace NShared.GetEggRewards;

public enum ResultType
{
	Fail,
	Success,
	NotFoundPetEggData,
	NotOpenSlot,
	InvalidItemData,
	[Comment("슬롯에 장착된 부화가기 아닙니다.")]
	IsNotMounting,
	[Comment("보유중인 부화기가 아닙니다.")]
	NotHaveIncubator,
	[Comment("부화 완료 시간이 아닙니다.")]
	NotMatchedItem
}
