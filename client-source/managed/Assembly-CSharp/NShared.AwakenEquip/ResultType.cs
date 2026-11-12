namespace NShared.AwakenEquip;

public enum ResultType
{
	Fail,
	Success,
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
	[Comment("각성 불가능한 장비입니다.")]
	ImpossibleAwaken,
	[Comment("각성 재료 요청 정보가 잘못되었습니다.")]
	MaterialItemInforsError,
	[Comment("재료 아이템의 아이템데이터를 찾을 수 없습니다.")]
	MaterialItemDataNotFound,
	[Comment("재료 아이템 보유 갯수가 부족합니다")]
	NotEnoughMaterialItem,
	[Comment("재료 아이템의 타입과 각성 아이템의 타입이 맞지 않습니다.")]
	NotMatchMaterialItemType,
	[Comment("각성석 데이터가 존재하지 않습니다.")]
	AwakenStoneDataNotFound,
	[Comment("재료 각성석으로 각성을 할 수 없는 장비가 포함되어있습니다.")]
	NotIncludeMaterialItemEquipItemIndices,
	[Comment("각성석으로 NPC 전용장비를 각성 할 수 없습니다.")]
	AwakenStoneNotAllowNPC
}
