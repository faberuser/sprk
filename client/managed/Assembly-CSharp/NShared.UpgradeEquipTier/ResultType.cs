namespace NShared.UpgradeEquipTier;

public enum ResultType
{
	Fail,
	Success,
	EquipNotOwned,
	EquipDataNotFound,
	NextTierEquipItemNotFound,
	InvalidNextTierStep,
	MaxTierItem,
	NotEnoughGold,
	NotEnoughGem,
	InvalidMaterialItem,
	MaterialItemDataNotFound,
	NotEnoughMaterialItem,
	UpgradeTierNotFound,
	ItemDataNotFound,
	EquipGradeDataNotFound,
	EquipDetailDataNotFound,
	NotEquipMaxStar,
	ContentsDisabled,
	[Comment("파견중인 영웅입니다.")]
	AlreadyOnBattleHero,
	[Comment("티어 업그레이드 불가능한 장비입니다.")]
	ImpossibleEquipItem,
	[Comment("미감정된 장비입니다.")]
	EquipNotIdentified
}
