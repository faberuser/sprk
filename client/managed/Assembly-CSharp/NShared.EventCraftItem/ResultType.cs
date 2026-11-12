namespace NShared.EventCraftItem;

public enum ResultType
{
	Fail,
	Success,
	ItemNotOwned,
	ItemDataNotFound,
	InvalidItemCount,
	[Comment("마녀의 신비한 솥의 레시피를 찾을 수 없습니다.")]
	EventCraftDataNotFound,
	[Comment("마녀의 신비한 솥의 제작 횟수 - 잘못된 입력값입니다.")]
	NotCorrectCraftCount,
	[Comment("이벤트 진행 조건이 없습니다.")]
	MissingBonusEventCondition,
	NotEnoughMaterial,
	[Comment("더이상 제작할 수 없습니다.")]
	NoLongerCraftable
}
