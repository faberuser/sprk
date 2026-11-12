namespace NShared.SetPetIncubator;

public enum ResultType
{
	Fail,
	Success,
	NotFoundIncubatorSlotData,
	NotFoundIncubatorData,
	NotOpenSlot,
	AlreadyMounting,
	[Comment("아직 부화가 진행중 입니다.")]
	HatchingInProgress,
	[Comment("보유중인 부화기가 아닙니다.")]
	NotHaveIncubator,
	ContentsDisabled
}
