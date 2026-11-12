namespace NShared.SetEquipStorageSlot;

public enum ResultType
{
	Fail,
	Success,
	NoHero,
	HeroDataNotFound,
	EquipNotOwned,
	AlreadyOtherHeroEquipped,
	ItemDataNotFound,
	EquipItemDataNotFound,
	EquipTagError,
	EquipCreatureError,
	TooLowGrade,
	TooLowLevel,
	LockedPartIndex,
	EquipPartError,
	RemoveFailed,
	NotEnoughGem,
	UnequipOldFailed,
	EquipFailed,
	[Comment("장비 장착 슬롯 테이블 데이터를 찾을 수 없습니다.")]
	EquipSlotDataNotFound,
	[Comment("해당 아이템의 착용 슬롯이 아닙니다.")]
	NotMatchSubType,
	UnequipFailed,
	InvalidEquipStorageSlotIndex
}
