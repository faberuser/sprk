namespace NShared;

[JsonSerializable(Javascript = true)]
public enum AchievementRewardType
{
	None,
	[AchievementRewardType(CurrencyType = CurrencyType.Gold)]
	Gold,
	[AchievementRewardType(CurrencyType = CurrencyType.Gem)]
	Gem,
	[AchievementRewardType]
	StaminaTeamLevel,
	[AchievementRewardType]
	GoldTeamLevel,
	[AchievementRewardType]
	Item,
	[AchievementRewardType(CurrencyType = CurrencyType.PvpCoin)]
	PvpCoin,
	[AchievementRewardType(CurrencyType = CurrencyType.TeamExp)]
	TeamExp,
	[AchievementRewardType(CurrencyType = CurrencyType.WorldBossPoint)]
	WorldBossPoint,
	[AchievementRewardType(CurrencyType = CurrencyType.EventDungeonTicket)]
	EventDungeonTicket,
	[AchievementRewardType(CurrencyType = CurrencyType.GloryPoint)]
	GloryPoint,
	[AchievementRewardType(CurrencyType = CurrencyType.EventDungeonPoint)]
	EventDungeonPoint,
	[AchievementRewardType(CurrencyType = CurrencyType.EventDungeonPoint2)]
	EventDungeonPoint2,
	[AchievementRewardType(CurrencyType = CurrencyType.Stamina)]
	Stamina,
	[AchievementRewardType]
	Booster,
	[AchievementRewardType(CurrencyType = CurrencyType.FriendshipPoint)]
	FriendshipPoint,
	[AchievementRewardType(CurrencyType = CurrencyType.RaidPoint)]
	RaidPoint,
	[AchievementRewardType(CurrencyType = CurrencyType.None)]
	Handler,
	[AchievementRewardType(CurrencyType = CurrencyType.EventOrvelPoint)]
	EventOrvelPoint
}
