namespace NShared.SetLockRuneItem;

public enum ResultType
{
	Fail,
	Success,
	ItemNotOwned,
	ItemDataNotFound,
	InvalidItemType,
	AlreadyLocked,
	AlreadyUnLocked
}
