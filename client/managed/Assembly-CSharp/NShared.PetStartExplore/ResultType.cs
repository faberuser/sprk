namespace NShared.PetStartExplore;

public enum ResultType
{
	Fail,
	Success,
	ContentsDisabled,
	InvalidValue,
	InfoError,
	NotHavePet,
	[Comment("탐사를 보낼 레이더 수가 부족합니다.")]
	NotEnoughExplorePetCount,
	[Comment("포만감이 부족합니다.")]
	NotEnoughFullPoint,
	[Comment("탐사를 보낼 수 없는 레이더 입니다.")]
	CannotExplorePet,
	[Comment("이미 탐사중 레이더 입니다.")]
	AlreadyExplorePet,
	[Comment("활동제한 중인 레이더 입니다.")]
	ActivityRestrictionPet,
	[Comment("탐사단 수가 초과했습니다.")]
	OverPetExploreCount,
	[Comment("이미 해당지역에 보낸 탐사단이 있습니다.")]
	AlreadyExistExplore
}
