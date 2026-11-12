namespace NShared.ValanceAwaken;

public enum ResultType
{
	Fail,
	Success,
	[Comment("잘못된 값으로 요청하였습니다.")]
	InvalidValue,
	EquipNotOwned,
	EquipDataNotFound,
	StatDataNotFound,
	EquipGradeDataNotFound,
	EquipDetailDataNotFound,
	CannotAwakenYet,
	CannotAwakenMore,
	EmptyMaterialEquipItem,
	MaterialEquipItemPriceError,
	NotEnoughGold,
	NotOwnedMaterialEquipItem,
	[Comment("소울웨폰은 재료로 사용 할 수 없습니다.")]
	MaterialEquipItemIsSoulWeapon,
	InvalidMaterialEquipItem,
	NoUpgradableMaterialEquipItem,
	LockedEquipItemExists,
	DifferentMaterialEquipItem,
	[Comment("미감정된 장비는 각성할 수 없습니다.")]
	EquipNotIdentified,
	[Comment("미감정된 장비재료는 사용할 수 없습니다.")]
	MaterialEquipNotIdentified,
	[Comment("재료가 충분하지 않습니다.")]
	NotEnoughItem,
	[Comment("각성 불가능한 장비입니다.")]
	ImpossibleAwaken,
	EquipItemFull
}
