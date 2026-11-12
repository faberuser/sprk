namespace NShared.SetChest;

public enum ResultType
{
	Fail,
	Success,
	EquipNotOwned,
	AlreadyOtherHeroEquipped,
	ItemDataNotFound,
	EquipItemDataNotFound,
	LockedPartIndex,
	EquipPartError,
	UnequipOldFailed,
	[Comment("장비 장착 슬롯 테이블 데이터를 찾을 수 없습니다.")]
	EquipSlotDataNotFound,
	[Comment("해당 아이템의 착용 슬롯이 아닙니다.")]
	NotMatchSubType,
	ChestFull
}
