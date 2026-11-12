namespace NShared.UseWeaponUniqueSelectItem;

public enum ResultType
{
	Fail,
	Success,
	ItemNotOwned,
	ItemDataNotFound,
	WeaponUniqueSelectDataNotFound,
	WeaponUniqueNotSelectable,
	ItemTypeMismatch,
	EquipItemDataNotFound,
	EquipItemFull,
	[Comment("아이템 사용 갯수가 최대치를 초과하였거나 0이하 입니다.")]
	ItemCountError,
	[Comment("게임데이터 값이 잘 못 되었습니다.")]
	WeaponUniqueSelectDataValueError
}
