namespace NShared.SetLockEquipItem;

public enum ResultType
{
	Fail,
	Success,
	EquipNotOwned,
	ItemDataNotFound,
	EquipItemDataNotFound,
	AlreadyLocked,
	AlreadyUnLocked
}
