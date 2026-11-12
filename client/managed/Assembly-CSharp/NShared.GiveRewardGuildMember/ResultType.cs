namespace NShared.GiveRewardGuildMember;

public enum ResultType
{
	Fail,
	Success,
	[Comment("보상정보를 찾을 수 없습니다.")]
	PcAvartarDataNotFound,
	InvalidItemCode,
	EquipItemFull,
	InvalidItemIndex,
	InvalidItemType,
	ItemNotOwned,
	ContentsDisabled,
	[Comment("길드 맴버에게 보상받기에 이른 시간입니다.")]
	NotElapsedGuildMemberRewardedTime
}
