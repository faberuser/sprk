namespace NShared;

[JsonSerializable(Javascript = true)]
public enum StaminaType
{
	Empty = -1,
	None,
	[StaminaType(CurrencyType = CurrencyType.Stamina)]
	Chicken,
	[StaminaType(CurrencyType = CurrencyType.Sword)]
	Sword,
	[StaminaType(CurrencyType = CurrencyType.Gold)]
	Gold,
	[StaminaType(CurrencyType = CurrencyType.Gem)]
	Gem,
	[StaminaType(CurrencyType = CurrencyType.UndergroundPrisonKey)]
	UndergroundPrisonKey,
	[StaminaType(CurrencyType = CurrencyType.UndergroundLabyrinthKey)]
	UndergroundLabyrinthKey,
	[StaminaType(CurrencyType = CurrencyType.Mileage)]
	Mileage,
	[StaminaType(CurrencyType = CurrencyType.HideoutKey)]
	HideoutKey,
	[StaminaType(CurrencyType = CurrencyType.ChallengeTowerKey)]
	ChallengeTowerKey,
	[StaminaType(CurrencyType = CurrencyType.GuildRaidTicket)]
	GuildRaidTicket,
	[StaminaType(CurrencyType = CurrencyType.WorldBossTicket)]
	WorldBossTicket,
	[StaminaType(CurrencyType = CurrencyType.EventDungeonTicket)]
	EventDungeonTicket,
	[StaminaType(CurrencyType = CurrencyType.Sword2)]
	Sword2,
	[StaminaType(CurrencyType = CurrencyType.MazeKey)]
	MazeKey,
	[StaminaType(CurrencyType = CurrencyType.GuildSuppressKey)]
	GuildSuppressKey,
	[StaminaType(CurrencyType = CurrencyType.TreasureHouseKey)]
	TreasureHouseKey,
	[StaminaType(CurrencyType = CurrencyType.EventWorldBossTicket)]
	EventWorldBossTicket,
	[StaminaType(CurrencyType = CurrencyType.WorldBossGlobalTicket)]
	WorldBossGlobalTicket,
	[StaminaType(CurrencyType = CurrencyType.ConquestKey)]
	ConquestKey,
	[StaminaType(CurrencyType = CurrencyType.GuildArenaKey)]
	GuildArenaKey,
	[StaminaType(CurrencyType = CurrencyType.GodkingTrialKey)]
	GodkingTrialKey,
	[StaminaType(CurrencyType = CurrencyType.EclipseKey)]
	EclipseKey,
	[StaminaType(CurrencyType = CurrencyType.EventHeroDungeonTicket)]
	EventHeroDungeonTicket,
	[StaminaType(CurrencyType = CurrencyType.EventDungeonGoldKey)]
	EventDungeonGoldKey,
	[StaminaType(CurrencyType = CurrencyType.PrisonKey)]
	PrisonKey,
	[StaminaType(CurrencyType = CurrencyType.ShakmehMiddleBossKey)]
	ShakmehMiddleBossKey,
	[StaminaType(CurrencyType = CurrencyType.PetFeedKey)]
	PetFeedKey,
	[StaminaType(CurrencyType = CurrencyType.Sword3)]
	Sword3,
	[StaminaType(CurrencyType = CurrencyType.TechnoEnchantKey)]
	TechnoEnchantKey,
	[StaminaType(CurrencyType = CurrencyType.PunishmentRaidKey)]
	PunishmentRaidKey
}
