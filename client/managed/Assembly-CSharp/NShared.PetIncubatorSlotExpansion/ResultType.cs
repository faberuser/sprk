namespace NShared.PetIncubatorSlotExpansion;

public enum ResultType
{
	Fail,
	Success,
	[Comment("부화기 슬롯 정보가 존재하지 않습니다.")]
	NotPetIncubatorSlotInfos,
	[Comment("마지막 슬롯까지 이미 열려 있습니다.")]
	AlreadyOpenUntilTheLastSlot,
	NotFoundIncubatorSlotData,
	NotEnoughCost,
	ContentsDisabled
}
