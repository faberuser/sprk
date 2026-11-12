namespace NShared.FeedThePet;

public enum ResultType
{
	Fail,
	Success,
	CreatureDataNotFound,
	[Comment("라인프렌즈 레이더는 이 행동을 할 수없습니다.")]
	IsLinePet,
	[Comment("레이더를 보유하고 있지 않습니다.")]
	NotHavePet,
	[Comment("레이더가 기본상태가 아닙니다.")]
	PetStatusIsNotNormal,
	[Comment("활동제한 시간이 지나지 않았습니다.")]
	PenaltyTimeNotPass,
	[Comment("레이더 먹이가 부족합니다.")]
	NotEnoughFeed,
	[Comment("최대 포만감 이상의 먹이를 한번에 먹일수 없습니다.")]
	OverFoodCount,
	[Comment("이미 포만감이 최대치입니다.")]
	MaxFullPoint,
	ContentsDisabled
}
