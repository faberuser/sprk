namespace NShared.UpgradeEquip;

public enum ResultType
{
	Fail,
	Success,
	EquipNotOwned,
	EquipDataNotFound,
	StatDataNotFound,
	CannotUpgradeMore,
	EmptyMaterialEquipItem,
	MaterialEquipItemPriceError,
	NotEnoughGold,
	NotOwnedMaterialEquipItem,
	InvalidMaterialItem,
	InvalidMaterialEquipItem,
	NoUpgradableMaterialEquipItem,
	InvalidReqItemExpData,
	ExceededMaxMaterialEquipItems,
	HeroEquippedMaterialEquipItem,
	LockedEquipItemExists,
	[Comment("장비의 최대 강화 수치를 초과하였습니다.")]
	ExceededMaxLevel,
	[Comment("강화를 실행 할 수 없는 값입니다.")]
	CanNotUpgradeValue,
	[Comment("파견중인 영웅입니다.")]
	AlreadyOnBattleHero
}
