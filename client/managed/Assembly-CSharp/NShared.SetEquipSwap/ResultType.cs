namespace NShared.SetEquipSwap;

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
	[Comment("교체하려는 장비가 2개가 아닙니다.")]
	NotCorrectEquipSwapCount,
	[Comment("교체하려는 장비가 장착상태가 아닙니다.")]
	HeroNotEquipped,
	[Comment("파견중인 영웅입니다.")]
	AlreadyOnBattleHero
}
