namespace NShared.RecvRewardFriendlyPoint;

public enum ResultType
{
	Fail,
	Success,
	CreatureDataNotFound,
	FriendlyPointInfoNotFound,
	FriendlyPointDataNotFound,
	InvalidFriendlyPointStep,
	NotEnoughFriendlyPoint,
	InvalidRewardIndex,
	ContentsDisabled,
	[Comment("더이상 보상을 수령 할 수 없습니다.")]
	CanNotLongerReceiveRewards
}
