namespace NShared.EventForge;

public enum ResultType
{
	Fail,
	Success,
	EquipNotOwned,
	EquipDataNotFound,
	StatDataNotFound,
	EquipGradeDataNotFound,
	EquipDetailDataNotFound,
	EmptyMaterialEquipItem,
	MaterialEquipItemPriceError,
	NotEnoughGold,
	CannotUpgradeMore,
	[Comment("장비의 최대 강화 수치를 초과하였습니다.")]
	ExceededMaxLevel,
	[Comment("강화를 실행 할 수 없는 값입니다.")]
	CanNotUpgradeValue,
	NotOwnedMaterialEquipItem,
	InvalidMaterialEquipItem,
	NoUpgradableMaterialEquipItem,
	LockedEquipItemExists,
	DifferentMaterialEquipItem,
	[Comment("각성 재료 요청 정보가 잘못되었습니다.")]
	MaterialItemInforsError,
	[Comment("재료 아이템의 아이템데이터를 찾을 수 없습니다.")]
	MaterialItemDataNotFound,
	[Comment("재료 아이템 보유 갯수가 부족합니다")]
	NotEnoughMaterialItem,
	[Comment("재료 아이템의 타입과 각성 아이템의 타입이 맞지 않습니다.")]
	NotMatchMaterialItemType
}
