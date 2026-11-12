namespace NShared.CancelCraftItem;

public enum ResultType
{
	Fail,
	Success,
	WrongSlotIndex,
	NotSelectableSlotIndex,
	NotCraftSlotIndex,
	AlreadyCompletedCraftItem,
	ItemDataNotFound,
	CraftDataNotFound,
	NotEnoughMaterial,
	NotEnoughGold
}
