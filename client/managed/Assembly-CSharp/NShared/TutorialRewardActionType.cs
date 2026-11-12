namespace NShared;

[JsonSerializable(Javascript = true)]
public enum TutorialRewardActionType
{
	None,
	SetHeroEquip,
	SetHeroStar,
	AddHeroExp,
	UpgradeHeroEquip,
	SetHeroSkillLevel,
	SetHeroSkillExtend,
	DecFreeEquipGachaCount,
	EquipRune,
	AddRaidPoint,
	AddCustomEquipItem
}
