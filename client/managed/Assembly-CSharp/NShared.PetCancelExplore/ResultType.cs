namespace NShared.PetCancelExplore;

public enum ResultType
{
	Fail,
	Success,
	ContentsDisabled,
	InvalidValue,
	InfoError,
	[Comment("탐사단이 존재하지 않습니다.")]
	NotExistExplore
}
