namespace NShared.UseEquipOptionSelectItem;

public enum ResultType
{
	Fail,
	Success,
	ItemNotOwned,
	ItemDataNotFound,
	ItemTypeMismatch,
	WeaponUniqueSelectDataNotFound,
	WeaponUniqueNotSelectable,
	EquipItemDataNotFound,
	EquipItemFull,
	NoAvailableOption,
	NotCorrectOptionCount,
	[Comment("아이템 사용 갯수가 최대치를 초과하였거나 0이하 입니다.")]
	ItemCountError
}
