namespace NShared;

public enum EventBonusType
{
	None,
	[EventBonus(PercentPrefix = "Exp")]
	BonusExp,
	[EventBonus(PercentPrefix = "Loot")]
	BonusItem,
	[EventBonus(PercentPrefix = "Gold")]
	BonusGold,
	[EventBonus(PercentPrefix = "Count")]
	BonusStamina,
	[EventBonus(PercentPrefix = "Count")]
	BonusEventDungeonCount,
	[EventBonus(PercentPrefix = "Point")]
	BonusFriendlyPoint,
	[EventBonus(PercentPrefix = "Discount")]
	DiscountPurify,
	[EventBonus(PercentPrefix = "Rate")]
	MulItemGachaRate,
	[EventBonus(PercentPrefix = "Discount")]
	DiscountKeepRune,
	[EventBonus(PercentPrefix = "Discount")]
	DiscountAwakeEquip,
	[EventBonus(PercentPrefix = "Point")]
	BonusRaidPoint,
	[EventBonus(PercentPrefix = "")]
	OpenAllPrison,
	[EventBonus(PercentPrefix = "Discount")]
	DiscountUpgradeOption,
	[EventBonus(PercentPrefix = "")]
	EquipGachaReward,
	[EventBonus(PercentPrefix = "HotTime")]
	HotTime,
	[EventBonus(PercentPrefix = "Rate")]
	PaybackGem,
	[EventBonus(PercentPrefix = "")]
	EventNode,
	[EventBonus(PercentPrefix = "")]
	EventShop,
	[EventBonus(PercentPrefix = "Discount")]
	DiscountShop,
	[EventBonus(PercentPrefix = "")]
	EventDungeon,
	[EventBonus(PercentPrefix = "")]
	EventReward,
	[EventBonus(PercentPrefix = "Rate")]
	BonusWorldBossCurrency,
	[EventBonus(PercentPrefix = "")]
	ChallengeRaidSeason,
	[EventBonus(PercentPrefix = "")]
	ChallengeRaid,
	[EventBonus(PercentPrefix = "")]
	ChallengeRaidNotice,
	[EventBonus(PercentPrefix = "")]
	EventWorldBossSeason,
	[EventBonus(PercentPrefix = "")]
	BanPickSeason,
	[EventBonus(PercentPrefix = "")]
	ShopList,
	[EventBonus(PercentPrefix = "Count")]
	BonusHideoutDungeonCount,
	[EventBonus(PercentPrefix = "Discount")]
	DiscountEquipItemOptionRenew,
	[EventBonus(PercentPrefix = "Discount")]
	DiscountEquipItemOptionUpgrade,
	[EventBonus(PercentPrefix = "Count")]
	BonusRaidReward,
	[EventBonus(PercentPrefix = "Rate")]
	BonusDailyMissionReward,
	[EventBonus(PercentPrefix = "Point")]
	BonusClosenessPoint,
	[EventBonus(PercentPrefix = "Rate")]
	MulItemGachaRateTreasure,
	[EventBonus(PercentPrefix = "Rate")]
	BonusEquipSellReward,
	[EventBonus(PercentPrefix = "Rate")]
	BonusEquipBreakReward,
	[EventBonus(PercentPrefix = "Count")]
	BonusEquipCostumeType,
	[EventBonus(PercentPrefix = "")]
	EventWorldBossReward,
	[EventBonus(PercentPrefix = "")]
	WorldBossGlobalRankingReward,
	[EventBonus(PercentPrefix = "Count")]
	NpcBuff,
	[EventBonus(PercentPrefix = "Count")]
	AddStamina,
	[EventBonus(PercentPrefix = "Count")]
	AccountBuff,
	[EventBonus(PercentPrefix = "Count")]
	UnderPrisonLimit,
	[EventBonus(PercentPrefix = "")]
	EventWorldBoss,
	[EventBonus(PercentPrefix = "")]
	BonusEventMissionReward,
	[EventBonus(PercentPrefix = "Rate")]
	SaveStamina,
	[EventBonus(PercentPrefix = "Count")]
	AddReward,
	[EventBonus(PercentPrefix = "")]
	ClassBuff,
	[EventBonus(PercentPrefix = "Rate")]
	BonusHousingObjectSellReward,
	[EventBonus(PercentPrefix = "Rate")]
	BonusAssignedItem,
	[EventBonus(PercentPrefix = "")]
	LimitedHeroEventMission,
	[EventBonus(PercentPrefix = "")]
	LimitedEquipEventMission,
	[EventBonus(PercentPrefix = "Discount")]
	DiscountSoulWeaponUpgrade,
	[EventBonus(PercentPrefix = "Rate")]
	BonusSoulBreakReward,
	[EventBonus(PercentPrefix = "Discount")]
	DiscountSoulWeaponOptionRenew,
	[EventBonus(PercentPrefix = "Discount")]
	DiscountPurchaseDungeon,
	[EventBonus(PercentPrefix = "Discount")]
	PurchaseDungeon,
	[EventBonus(PercentPrefix = "Count")]
	BonusDungeonItemCount,
	[EventBonus(PercentPrefix = "Rate")]
	BonusEclipsePoint,
	[EventBonus(PercentPrefix = "Discount")]
	DiscountSoulWeaponEther,
	[EventBonus(PercentPrefix = "Rate")]
	DecRegenStaminaTime,
	[EventBonus(PercentPrefix = "")]
	PetBuff,
	[EventBonus(PercentPrefix = "Count")]
	AddEnterPrisonDungeon,
	[EventBonus(PercentPrefix = "")]
	EventRoulette,
	[EventBonus(PercentPrefix = "")]
	EventCraft,
	[EventBonus(PercentPrefix = "")]
	KingsPass,
	[EventBonus(PercentPrefix = "")]
	StaminaConsumePoint,
	[EventBonus(PercentPrefix = "Discount")]
	DiscountMaterialItemCount,
	[EventBonus(PercentPrefix = "Discount")]
	DiscountAdditionalMaterialItemCount,
	[EventBonus(PercentPrefix = "Discount")]
	DiscountSubMaterialItemCount,
	[EventBonus(PercentPrefix = "Discount")]
	DiscountPetIncubatorExpansion,
	[EventBonus(PercentPrefix = "Discount")]
	DiscountBuyPetIncubator,
	[EventBonus(PercentPrefix = "")]
	GrowWorldTree,
	[EventBonus(PercentPrefix = "")]
	PayShopDiscount,
	[EventBonus(PercentPrefix = "Discount")]
	SaveDispatchStamina,
	[EventBonus(PercentPrefix = "Rate")]
	GuildArenaAttackSuccessReward
}
