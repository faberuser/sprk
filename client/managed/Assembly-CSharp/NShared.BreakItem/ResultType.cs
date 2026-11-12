namespace NShared.BreakItem;

public enum ResultType
{
	Fail,
	Success,
	ItemNotOwned,
	InvalidItemCount,
	Equipped,
	NotBreakableItemExists,
	LockedItemExists,
	ItemDataNotFound,
	[Comment("아이템분해 데이터가 존재하지 않습니다.")]
	ItemBreakDataNotFound,
	InvalidItemType
}
