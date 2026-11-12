namespace NShared.SoulWeaponLimitBreak;

public enum ResultType
{
	Fail,
	Success,
	EquipNotOwned,
	EquipDataNotFound,
	MaterialEquipItemPriceError,
	NotEnoughGold,
	LockedEquipItemExists,
	DifferentMaterialEquipItem,
	[Comment("재료 아이템의 아이템데이터를 찾을 수 없습니다.")]
	MaterialItemDataNotFound,
	[Comment("재료 아이템 보유 갯수가 부족합니다")]
	NotEnoughMaterialItem,
	InvalidValue,
	CantUsableEquipItem
}
