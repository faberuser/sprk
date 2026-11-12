namespace NShared.GetClearMissionReward;

public enum ResultType
{
	Fail,
	Success,
	[Comment("잘못된 돌파미션 번호입니다.")]
	InvalidClearMissionIndex,
	[Comment("잘못된 돌파미션 보상 단계입니다.")]
	InvalidClearMissionStep,
	[Comment("완료되지 않은 돌파미션입니다.")]
	ClearMissionNotCleared,
	[Comment("보상 수령 조건을 만족하지 못하였습니다.")]
	CheckConditionNotPassed
}
