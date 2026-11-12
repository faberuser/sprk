namespace NShared.GetNewbieMissionReward;

public enum ResultType
{
	Fail,
	Success,
	[Comment("보상 수령 조건을 만족하지 못하였습니다.")]
	CheckConditionNotPassed
}
