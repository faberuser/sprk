namespace NShared.GetEventStepInfo;

public enum ResultType
{
	Fail,
	Success,
	[Comment("미진행 기간 입니다.")]
	PortalNotgoaheadContents,
	[Comment("해당 컨텐츠가 열려있지 않습니다.")]
	ContentsDisabled,
	PlayerNotFound
}
