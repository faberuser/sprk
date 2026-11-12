namespace NShared.GiveGiftItem;

public enum ResultType
{
	Fail,
	Success,
	InvalidItemData,
	MaxFriendlyPoint,
	CannotGiveGiftNpc,
	CreatureDataNotFound,
	ContentsDisabled,
	FriendlyPointInfoNotFound,
	[Comment("NPCFriendlyPoint 게임 데이터를 찾을 수 없습니다.")]
	NPCFriendlyPointDataNotFound,
	[Comment("선물할 수 없는 아이템입니다.")]
	CannotGiveRestrictionItem,
	[Comment("한번에 한단계씩만 단계 상승이 가능합니다.")]
	RequestedMoreThanOneStep,
	[Comment("이전 단계 보상을 먼저 수령 받아야 합니다.")]
	FirstGetPreviousRewards
}
