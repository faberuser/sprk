namespace NShared;

[JsonSerializable(Javascript = true, ParseField = true)]
public class UserInfo
{
	[NonDbSchema]
	public string SessionKey { get; set; }

	public long AccountId { get; set; }

	public string Nick { get; set; }

	public long Gold { get; set; }

	public long Gem { get; set; }

	public long PayGem { get; set; }

	public long PvpCoin { get; set; }

	public long Stamina { get; set; }

	public string StaminaRechargeTime { get; set; }

	public int TeamLevel { get; set; }

	public int TeamExp { get; set; }

	public long Sword { get; set; }

	public string SwordRechargeTime { get; set; }

	public int SwordRechargeCount { get; set; }

	public long Sword2 { get; set; }

	public string Sword2RechargeTime { get; set; }

	public int AvatarHeroIndex { get; set; }

	public long RoyalPoint { get; set; }

	public long UndergroundPrisonKey { get; set; }

	public string UndergroundPrisonKeyRechargeTime { get; set; }

	public int UndergroundPrisonKeyRechargeCount { get; set; }

	public long UndergroundLabyrinthKey { get; set; }

	public string UndergroundLabyrinthKeyRechargeTime { get; set; }

	public long Mileage { get; set; }

	public long FriendshipPoint { get; set; }

	public long HideoutKey { get; set; }

	public string HideoutKeyRechargeTime { get; set; }

	public int HideoutKeyRechargeCount { get; set; }

	public long ChallengeTowerKey { get; set; }

	public string ChallengeTowerKeyRechargeTime { get; set; }

	public int ChallengeTowerKeyRechargeCount { get; set; }

	public int LastChapterIndex { get; set; }

	public int LastDungeonIndex { get; set; }

	public long RaidPoint { get; set; }

	public long GuildRaidTicket { get; set; }

	public string GuildRaidTicketRechargeTime { get; set; }

	public long WorldBossTicket { get; set; }

	public string WorldBossTicketRechargeTime { get; set; }

	public long WorldBossPoint { get; set; }

	public long EventDungeonTicket { get; set; }

	public string EventDungeonTicketRechargeTime { get; set; }

	public long EventDungeonPoint { get; set; }

	public long EventDungeonPoint2 { get; set; }

	public int EventDungeonTicketRechargeCount { get; set; }

	public long EventDungeonDailyPoint { get; set; }

	public string EventDungeonDailyPointResetTime { get; set; }

	public long GloryPoint { get; set; }

	public long EventGiftPoint { get; set; }

	public long LuaPoint { get; set; }

	public long MazeKey { get; set; }

	public string MazeKeyRechargeTime { get; set; }

	public int MazeKeyRechargeCount { get; set; }

	public string CreatedTime { get; set; }

	public long GuildSuppressKey { get; set; }

	public string GuildSuppressKeyRechargeTime { get; set; }

	public int GuildSuppressKeyRechargeCount { get; set; }

	public long GuildArenaKey { get; set; }

	public string GuildArenaKeyRechargeTime { get; set; }

	public int GuildArenaKeyRechargeCount { get; set; }

	public long TreasureHouseKey { get; set; }

	public string TreasureHouseKeyRechargeTime { get; set; }

	public int TreasureHouseKeyRechargeCount { get; set; }

	public long EventWorldBossTicket { get; set; }

	public string EventWorldBossTicketRechargeTime { get; set; }

	public long WorldBossGlobalTicket { get; set; }

	public string WorldBossGlobalTicketRechargeTime { get; set; }

	public long GodkingTrialKey { get; set; }

	public string GodkingTrialKeyRechargeTime { get; set; }

	public int GodkingTrialKeyRechargeCount { get; set; }

	public long EclipseKey { get; set; }

	public string EclipseKeyRechargeTime { get; set; }

	public int EclipseKeyRechargeCount { get; set; }

	public long EventHeroDungeonTicket { get; set; }

	public string EventHeroDungeonTicketRechargeTime { get; set; }

	public long EventDungeonGoldKey { get; set; }

	public string EventDungeonGoldKeyRechargeTime { get; set; }

	public long PrisonKey { get; set; }

	public string PrisonKeyRechargeTime { get; set; }

	public int PrisonKeyRechargeCount { get; set; }

	public long ShakmehMiddleBossKey { get; set; }

	public string ShakmehMiddleBossKeyRechargeTime { get; set; }

	[NonDbSchema]
	public long PetFeedKey { get; set; }

	[NonDbSchema]
	public string PetFeedRechargeTime { get; set; }

	[NonDbSchema]
	public int PetFeedRechargeCount { get; set; }

	[NonDbSchema]
	public long Sword3 { get; set; }

	[NonDbSchema]
	public string Sword3RechargeTime { get; set; }

	[NonDbSchema]
	public int Sword3RechargeCount { get; set; }

	[NonDbSchema]
	public long TechnoEnchantKey { get; set; }

	[NonDbSchema]
	public string TechnoEnchantKeyRechargeTime { get; set; }

	[NonDbSchema]
	public int TechnoEnchantKeyRechargeCount { get; set; }

	[NonDbSchema]
	public long EventOrvelPoint { get; set; }

	[NonDbSchema]
	public long PunishmentRaidKey { get; set; }

	[NonDbSchema]
	public string PunishmentRaidKeyRechargeTime { get; set; }

	[NonDbSchema]
	public int PunishmentRaidKeyRechargeCount { get; set; }
}
