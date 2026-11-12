namespace NShared.UnsetEggInPetIncubator;

public enum ResultType
{
	Fail,
	Success,
	NotOpenSlot,
	IsNotMounting,
	NotHaveIncubator,
	[Comment("부화기가 비어있습니다.")]
	EmptyIncubator,
	[Comment("부화가 완료된 상태에선 해체가 불가능합니다.")]
	CompletedHatching,
	ContentsDisabled
}
