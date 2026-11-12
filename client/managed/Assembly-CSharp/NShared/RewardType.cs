namespace NShared;

[JsonSerializable(Javascript = true)]
public enum RewardType
{
	None,
	Item,
	[RewardType(CurrencyType = CurrencyType.Gold)]
	Gold,
	[RewardType(CurrencyType = CurrencyType.Gem)]
	Gem,
	[RewardType(CurrencyType = CurrencyType.PvpCoin)]
	PvpCoin,
	[RewardType(CurrencyType = CurrencyType.GuildPoint)]
	GuildPoint,
	[RewardType(CurrencyType = CurrencyType.Mileage)]
	Mileage,
	[RewardType(CurrencyType = CurrencyType.WorldBossPoint)]
	WorldBossPoint,
	[RewardType(CurrencyType = CurrencyType.EventDungeonPoint)]
	EventDungeonPoint,
	[RewardType(CurrencyType = CurrencyType.EventDungeonPoint2)]
	EventDungeonPoint2,
	[RewardType(CurrencyType = CurrencyType.EventGiftPoint)]
	EventGiftPoint,
	[RewardType(CurrencyType = CurrencyType.GloryPoint)]
	GloryPoint,
	[RewardType(CurrencyType = CurrencyType.RaidPoint)]
	RaidPoint,
	[RewardType(CurrencyType = CurrencyType.LuaPoint)]
	LuaPoint,
	[RewardType(CurrencyType = CurrencyType.FriendshipPoint)]
	FriendshipPoint,
	[RewardType(CurrencyType = CurrencyType.RankingPoint)]
	RankingPoint,
	[RewardType(CurrencyType = CurrencyType.GuildArenaPoint)]
	GuildArenaPoint,
	[RewardType(CurrencyType = CurrencyType.None)]
	Handler,
	[RewardType(CurrencyType = CurrencyType.EclipsePoint)]
	EclipsePoint,
	[RewardType(CurrencyType = CurrencyType.ShopEventPoint)]
	ShopEventPoint,
	[RewardType(CurrencyType = CurrencyType.ShakmehMiddleBossPoint)]
	ShakmehMiddleBossPoint,
	[RewardType(CurrencyType = CurrencyType.LimitedShopEventPoint)]
	LimitedShopEventPoint,
	[RewardType(CurrencyType = CurrencyType.OrdealArenaPoint)]
	OrdealArenaPoint,
	[RewardType(CurrencyType = CurrencyType.PetFeedKey)]
	PetFeedKey,
	[RewardType(CurrencyType = CurrencyType.TreasureHousePoint)]
	TreasureHousePoint,
	[RewardType(CurrencyType = CurrencyType.EventOrvelPoint)]
	EventOrvelPoint,
	[RewardType(CurrencyType = CurrencyType.GrowWorldTreePoint)]
	GrowWorldTreePoint
}
