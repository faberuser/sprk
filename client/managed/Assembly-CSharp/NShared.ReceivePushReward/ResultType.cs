namespace NShared.ReceivePushReward;

public enum ResultType
{
	Fail,
	Success,
	[Comment("보상 받을 시간이 아닙니다.")]
	RewardTimeEnded
}
