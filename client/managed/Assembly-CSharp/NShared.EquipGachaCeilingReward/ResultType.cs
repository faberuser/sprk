namespace NShared.EquipGachaCeilingReward;

public enum ResultType
{
	Fail,
	Success,
	[Comment("보상을 받을 수 있는 조건이 아닙니다.")]
	NotEnoughCondition,
	ContentsDisabled,
	GachaDataNotFound,
	InvalidGachaRequest
}
