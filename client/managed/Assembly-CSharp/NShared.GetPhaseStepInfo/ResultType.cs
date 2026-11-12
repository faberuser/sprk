namespace NShared.GetPhaseStepInfo;

public enum ResultType
{
	Fail,
	Success,
	RequiredValueEmpty,
	[Comment("유효하지 않은 정보입니다.")]
	PhaseStepDataNotFound
}
