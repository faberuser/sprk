namespace NShared.PutEventStep;

public enum ResultType
{
	Fail,
	Success,
	[Comment("진행중인 이벤트가 아닙니다.")]
	PortalNotgoaheadContents,
	WrongStep,
	[Comment("잘못된 요청 입니다.")]
	InvalidValue,
	ContentsDisabled
}
