namespace NShared.BuyShopItem;

public enum ResultType
{
	Fail,
	Success,
	ShopDataNotFound,
	ShopItemDataNotFound,
	ItemDataNotFound,
	PlayerShopItemNotFound,
	SoldOut,
	NotEnoughGold,
	NotEnoughGem,
	NotEnoughPvpCoin,
	NotEnoughGuildPoint,
	InvalidCost,
	NotGuildMember,
	InvalidShopItemListIndex,
	InvalidItemType,
	NotEnoughRoyalPoint,
	ShopRestockInfoNotFound,
	EquipItemFull,
	HouseObjectFull,
	NotEnoughFriendshipPoint,
	NotEnoughRaidPoint,
	NoShopData,
	ContentsDisabled,
	NotEnoughWorldBossPoint,
	CannotBuyNoEvent,
	NotEnoughEventDungeonPoint,
	NotEnoughEventDungeonPoint2,
	NotEnoughGloryPoint,
	NotEnoughEventGift,
	NotEnoughLuaPoint,
	[Comment("영광의 휘장이 부족합니다.")]
	NotEnoughRankingPoint,
	NotEnoughEventDungeonPoint3,
	NotCorrectGuildBuildingLevel,
	[Comment("길드전 포인트가 부족합니다.")]
	NotEnoughGuildArenaPoint,
	[Comment("이벤트 재화가 부족합니다.")]
	NotEnoughShopEventPoint,
	[Comment("잘못된 값으로 요청하였습니다.")]
	InvalidValue,
	[Comment("시련의 전장 재화가 부족합니다.")]
	NotEnoughOrdealArenaPoint,
	[Comment("진행중인 첼린지레이드가 없습니다.")]
	NoCurrentChallengeRaid,
	[Comment("상점 정보가 일치하지 않습니다.")]
	NotMatchShopIndex,
	[Comment("재화가 부족 합니다.")]
	NotEnoughCost
}
