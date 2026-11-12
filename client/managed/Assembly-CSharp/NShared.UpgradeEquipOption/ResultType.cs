namespace NShared.UpgradeEquipOption;

public enum ResultType
{
	Fail,
	Success,
	EquipNotOwned,
	ItemDataNotFound,
	EquipDataNotFound,
	UpgradeDataNotFound,
	MaterialItemIndexMismatch,
	MaterialItemCountMismatch,
	NotEnoughMaterialItem,
	InvalidReqGold,
	NotEnoughGold,
	NoUpgradableOptionSlot,
	ContentsDisabled
}
