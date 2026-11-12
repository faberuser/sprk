using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_UserInfo
{
	public static UserInfo Parse(string s)
	{
		return Parse(s, new UserInfo());
	}

	public static UserInfo Parse(string s, UserInfo o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static UserInfo Parse(IDictionary d)
	{
		return Parse(d, new UserInfo());
	}

	public static UserInfo Parse(IDictionary d, UserInfo o)
	{
		o.SessionKey = BaseJsonMarshaler.ParseValueString(d, "SessionKey");
		o.AccountId = BaseJsonMarshaler.ParseValueLong(d, "AccountId");
		o.Nick = BaseJsonMarshaler.ParseValueString(d, "Nick");
		o.Gold = BaseJsonMarshaler.ParseValueLong(d, "Gold");
		o.Gem = BaseJsonMarshaler.ParseValueLong(d, "Gem");
		o.PayGem = BaseJsonMarshaler.ParseValueLong(d, "PayGem");
		o.PvpCoin = BaseJsonMarshaler.ParseValueLong(d, "PvpCoin");
		o.Stamina = BaseJsonMarshaler.ParseValueLong(d, "Stamina");
		o.StaminaRechargeTime = BaseJsonMarshaler.ParseValueString(d, "StaminaRechargeTime");
		o.TeamLevel = BaseJsonMarshaler.ParseValueInt(d, "TeamLevel");
		o.TeamExp = BaseJsonMarshaler.ParseValueInt(d, "TeamExp");
		o.Sword = BaseJsonMarshaler.ParseValueLong(d, "Sword");
		o.SwordRechargeTime = BaseJsonMarshaler.ParseValueString(d, "SwordRechargeTime");
		o.SwordRechargeCount = BaseJsonMarshaler.ParseValueInt(d, "SwordRechargeCount");
		o.Sword2 = BaseJsonMarshaler.ParseValueLong(d, "Sword2");
		o.Sword2RechargeTime = BaseJsonMarshaler.ParseValueString(d, "Sword2RechargeTime");
		o.AvatarHeroIndex = BaseJsonMarshaler.ParseValueInt(d, "AvatarHeroIndex");
		o.RoyalPoint = BaseJsonMarshaler.ParseValueLong(d, "RoyalPoint");
		o.UndergroundPrisonKey = BaseJsonMarshaler.ParseValueLong(d, "UndergroundPrisonKey");
		o.UndergroundPrisonKeyRechargeTime = BaseJsonMarshaler.ParseValueString(d, "UndergroundPrisonKeyRechargeTime");
		o.UndergroundPrisonKeyRechargeCount = BaseJsonMarshaler.ParseValueInt(d, "UndergroundPrisonKeyRechargeCount");
		o.UndergroundLabyrinthKey = BaseJsonMarshaler.ParseValueLong(d, "UndergroundLabyrinthKey");
		o.UndergroundLabyrinthKeyRechargeTime = BaseJsonMarshaler.ParseValueString(d, "UndergroundLabyrinthKeyRechargeTime");
		o.Mileage = BaseJsonMarshaler.ParseValueLong(d, "Mileage");
		o.FriendshipPoint = BaseJsonMarshaler.ParseValueLong(d, "FriendshipPoint");
		o.HideoutKey = BaseJsonMarshaler.ParseValueLong(d, "HideoutKey");
		o.HideoutKeyRechargeTime = BaseJsonMarshaler.ParseValueString(d, "HideoutKeyRechargeTime");
		o.HideoutKeyRechargeCount = BaseJsonMarshaler.ParseValueInt(d, "HideoutKeyRechargeCount");
		o.ChallengeTowerKey = BaseJsonMarshaler.ParseValueLong(d, "ChallengeTowerKey");
		o.ChallengeTowerKeyRechargeTime = BaseJsonMarshaler.ParseValueString(d, "ChallengeTowerKeyRechargeTime");
		o.ChallengeTowerKeyRechargeCount = BaseJsonMarshaler.ParseValueInt(d, "ChallengeTowerKeyRechargeCount");
		o.LastChapterIndex = BaseJsonMarshaler.ParseValueInt(d, "LastChapterIndex");
		o.LastDungeonIndex = BaseJsonMarshaler.ParseValueInt(d, "LastDungeonIndex");
		o.RaidPoint = BaseJsonMarshaler.ParseValueLong(d, "RaidPoint");
		o.GuildRaidTicket = BaseJsonMarshaler.ParseValueLong(d, "GuildRaidTicket");
		o.GuildRaidTicketRechargeTime = BaseJsonMarshaler.ParseValueString(d, "GuildRaidTicketRechargeTime");
		o.WorldBossTicket = BaseJsonMarshaler.ParseValueLong(d, "WorldBossTicket");
		o.WorldBossTicketRechargeTime = BaseJsonMarshaler.ParseValueString(d, "WorldBossTicketRechargeTime");
		o.WorldBossPoint = BaseJsonMarshaler.ParseValueLong(d, "WorldBossPoint");
		o.EventDungeonTicket = BaseJsonMarshaler.ParseValueLong(d, "EventDungeonTicket");
		o.EventDungeonTicketRechargeTime = BaseJsonMarshaler.ParseValueString(d, "EventDungeonTicketRechargeTime");
		o.EventDungeonPoint = BaseJsonMarshaler.ParseValueLong(d, "EventDungeonPoint");
		o.EventDungeonPoint2 = BaseJsonMarshaler.ParseValueLong(d, "EventDungeonPoint2");
		o.EventDungeonTicketRechargeCount = BaseJsonMarshaler.ParseValueInt(d, "EventDungeonTicketRechargeCount");
		o.EventDungeonDailyPoint = BaseJsonMarshaler.ParseValueLong(d, "EventDungeonDailyPoint");
		o.EventDungeonDailyPointResetTime = BaseJsonMarshaler.ParseValueString(d, "EventDungeonDailyPointResetTime");
		o.GloryPoint = BaseJsonMarshaler.ParseValueLong(d, "GloryPoint");
		o.EventGiftPoint = BaseJsonMarshaler.ParseValueLong(d, "EventGiftPoint");
		o.LuaPoint = BaseJsonMarshaler.ParseValueLong(d, "LuaPoint");
		o.MazeKey = BaseJsonMarshaler.ParseValueLong(d, "MazeKey");
		o.MazeKeyRechargeTime = BaseJsonMarshaler.ParseValueString(d, "MazeKeyRechargeTime");
		o.MazeKeyRechargeCount = BaseJsonMarshaler.ParseValueInt(d, "MazeKeyRechargeCount");
		o.CreatedTime = BaseJsonMarshaler.ParseValueString(d, "CreatedTime");
		o.GuildSuppressKey = BaseJsonMarshaler.ParseValueLong(d, "GuildSuppressKey");
		o.GuildSuppressKeyRechargeTime = BaseJsonMarshaler.ParseValueString(d, "GuildSuppressKeyRechargeTime");
		o.GuildSuppressKeyRechargeCount = BaseJsonMarshaler.ParseValueInt(d, "GuildSuppressKeyRechargeCount");
		o.GuildArenaKey = BaseJsonMarshaler.ParseValueLong(d, "GuildArenaKey");
		o.GuildArenaKeyRechargeTime = BaseJsonMarshaler.ParseValueString(d, "GuildArenaKeyRechargeTime");
		o.GuildArenaKeyRechargeCount = BaseJsonMarshaler.ParseValueInt(d, "GuildArenaKeyRechargeCount");
		o.TreasureHouseKey = BaseJsonMarshaler.ParseValueLong(d, "TreasureHouseKey");
		o.TreasureHouseKeyRechargeTime = BaseJsonMarshaler.ParseValueString(d, "TreasureHouseKeyRechargeTime");
		o.TreasureHouseKeyRechargeCount = BaseJsonMarshaler.ParseValueInt(d, "TreasureHouseKeyRechargeCount");
		o.EventWorldBossTicket = BaseJsonMarshaler.ParseValueLong(d, "EventWorldBossTicket");
		o.EventWorldBossTicketRechargeTime = BaseJsonMarshaler.ParseValueString(d, "EventWorldBossTicketRechargeTime");
		o.WorldBossGlobalTicket = BaseJsonMarshaler.ParseValueLong(d, "WorldBossGlobalTicket");
		o.WorldBossGlobalTicketRechargeTime = BaseJsonMarshaler.ParseValueString(d, "WorldBossGlobalTicketRechargeTime");
		o.GodkingTrialKey = BaseJsonMarshaler.ParseValueLong(d, "GodkingTrialKey");
		o.GodkingTrialKeyRechargeTime = BaseJsonMarshaler.ParseValueString(d, "GodkingTrialKeyRechargeTime");
		o.GodkingTrialKeyRechargeCount = BaseJsonMarshaler.ParseValueInt(d, "GodkingTrialKeyRechargeCount");
		o.EclipseKey = BaseJsonMarshaler.ParseValueLong(d, "EclipseKey");
		o.EclipseKeyRechargeTime = BaseJsonMarshaler.ParseValueString(d, "EclipseKeyRechargeTime");
		o.EclipseKeyRechargeCount = BaseJsonMarshaler.ParseValueInt(d, "EclipseKeyRechargeCount");
		o.EventHeroDungeonTicket = BaseJsonMarshaler.ParseValueLong(d, "EventHeroDungeonTicket");
		o.EventHeroDungeonTicketRechargeTime = BaseJsonMarshaler.ParseValueString(d, "EventHeroDungeonTicketRechargeTime");
		o.EventDungeonGoldKey = BaseJsonMarshaler.ParseValueLong(d, "EventDungeonGoldKey");
		o.EventDungeonGoldKeyRechargeTime = BaseJsonMarshaler.ParseValueString(d, "EventDungeonGoldKeyRechargeTime");
		o.PrisonKey = BaseJsonMarshaler.ParseValueLong(d, "PrisonKey");
		o.PrisonKeyRechargeTime = BaseJsonMarshaler.ParseValueString(d, "PrisonKeyRechargeTime");
		o.PrisonKeyRechargeCount = BaseJsonMarshaler.ParseValueInt(d, "PrisonKeyRechargeCount");
		o.ShakmehMiddleBossKey = BaseJsonMarshaler.ParseValueLong(d, "ShakmehMiddleBossKey");
		o.ShakmehMiddleBossKeyRechargeTime = BaseJsonMarshaler.ParseValueString(d, "ShakmehMiddleBossKeyRechargeTime");
		o.PetFeedKey = BaseJsonMarshaler.ParseValueLong(d, "PetFeedKey");
		o.PetFeedRechargeTime = BaseJsonMarshaler.ParseValueString(d, "PetFeedRechargeTime");
		o.PetFeedRechargeCount = BaseJsonMarshaler.ParseValueInt(d, "PetFeedRechargeCount");
		o.Sword3 = BaseJsonMarshaler.ParseValueLong(d, "Sword3");
		o.Sword3RechargeTime = BaseJsonMarshaler.ParseValueString(d, "Sword3RechargeTime");
		o.Sword3RechargeCount = BaseJsonMarshaler.ParseValueInt(d, "Sword3RechargeCount");
		o.TechnoEnchantKey = BaseJsonMarshaler.ParseValueLong(d, "TechnoEnchantKey");
		o.TechnoEnchantKeyRechargeTime = BaseJsonMarshaler.ParseValueString(d, "TechnoEnchantKeyRechargeTime");
		o.TechnoEnchantKeyRechargeCount = BaseJsonMarshaler.ParseValueInt(d, "TechnoEnchantKeyRechargeCount");
		o.EventOrvelPoint = BaseJsonMarshaler.ParseValueLong(d, "EventOrvelPoint");
		o.PunishmentRaidKey = BaseJsonMarshaler.ParseValueLong(d, "PunishmentRaidKey");
		o.PunishmentRaidKeyRechargeTime = BaseJsonMarshaler.ParseValueString(d, "PunishmentRaidKeyRechargeTime");
		o.PunishmentRaidKeyRechargeCount = BaseJsonMarshaler.ParseValueInt(d, "PunishmentRaidKeyRechargeCount");
		return o;
	}

	public static bool ParseField<T>(UserInfo o, string f, out T v)
	{
		v = default(T);
		switch (f)
		{
		case "SessionKey":
			v = BaseJsonMarshaler.To<T>(o.SessionKey);
			return true;
		case "AccountId":
			v = BaseJsonMarshaler.To<T>(o.AccountId);
			return true;
		case "Nick":
			v = BaseJsonMarshaler.To<T>(o.Nick);
			return true;
		case "Gold":
			v = BaseJsonMarshaler.To<T>(o.Gold);
			return true;
		case "Gem":
			v = BaseJsonMarshaler.To<T>(o.Gem);
			return true;
		case "PayGem":
			v = BaseJsonMarshaler.To<T>(o.PayGem);
			return true;
		case "PvpCoin":
			v = BaseJsonMarshaler.To<T>(o.PvpCoin);
			return true;
		case "Stamina":
			v = BaseJsonMarshaler.To<T>(o.Stamina);
			return true;
		case "StaminaRechargeTime":
			v = BaseJsonMarshaler.To<T>(o.StaminaRechargeTime);
			return true;
		case "TeamLevel":
			v = BaseJsonMarshaler.To<T>(o.TeamLevel);
			return true;
		case "TeamExp":
			v = BaseJsonMarshaler.To<T>(o.TeamExp);
			return true;
		case "Sword":
			v = BaseJsonMarshaler.To<T>(o.Sword);
			return true;
		case "SwordRechargeTime":
			v = BaseJsonMarshaler.To<T>(o.SwordRechargeTime);
			return true;
		case "SwordRechargeCount":
			v = BaseJsonMarshaler.To<T>(o.SwordRechargeCount);
			return true;
		case "Sword2":
			v = BaseJsonMarshaler.To<T>(o.Sword2);
			return true;
		case "Sword2RechargeTime":
			v = BaseJsonMarshaler.To<T>(o.Sword2RechargeTime);
			return true;
		case "AvatarHeroIndex":
			v = BaseJsonMarshaler.To<T>(o.AvatarHeroIndex);
			return true;
		case "RoyalPoint":
			v = BaseJsonMarshaler.To<T>(o.RoyalPoint);
			return true;
		case "UndergroundPrisonKey":
			v = BaseJsonMarshaler.To<T>(o.UndergroundPrisonKey);
			return true;
		case "UndergroundPrisonKeyRechargeTime":
			v = BaseJsonMarshaler.To<T>(o.UndergroundPrisonKeyRechargeTime);
			return true;
		case "UndergroundPrisonKeyRechargeCount":
			v = BaseJsonMarshaler.To<T>(o.UndergroundPrisonKeyRechargeCount);
			return true;
		case "UndergroundLabyrinthKey":
			v = BaseJsonMarshaler.To<T>(o.UndergroundLabyrinthKey);
			return true;
		case "UndergroundLabyrinthKeyRechargeTime":
			v = BaseJsonMarshaler.To<T>(o.UndergroundLabyrinthKeyRechargeTime);
			return true;
		case "Mileage":
			v = BaseJsonMarshaler.To<T>(o.Mileage);
			return true;
		case "FriendshipPoint":
			v = BaseJsonMarshaler.To<T>(o.FriendshipPoint);
			return true;
		case "HideoutKey":
			v = BaseJsonMarshaler.To<T>(o.HideoutKey);
			return true;
		case "HideoutKeyRechargeTime":
			v = BaseJsonMarshaler.To<T>(o.HideoutKeyRechargeTime);
			return true;
		case "HideoutKeyRechargeCount":
			v = BaseJsonMarshaler.To<T>(o.HideoutKeyRechargeCount);
			return true;
		case "ChallengeTowerKey":
			v = BaseJsonMarshaler.To<T>(o.ChallengeTowerKey);
			return true;
		case "ChallengeTowerKeyRechargeTime":
			v = BaseJsonMarshaler.To<T>(o.ChallengeTowerKeyRechargeTime);
			return true;
		case "ChallengeTowerKeyRechargeCount":
			v = BaseJsonMarshaler.To<T>(o.ChallengeTowerKeyRechargeCount);
			return true;
		case "LastChapterIndex":
			v = BaseJsonMarshaler.To<T>(o.LastChapterIndex);
			return true;
		case "LastDungeonIndex":
			v = BaseJsonMarshaler.To<T>(o.LastDungeonIndex);
			return true;
		case "RaidPoint":
			v = BaseJsonMarshaler.To<T>(o.RaidPoint);
			return true;
		case "GuildRaidTicket":
			v = BaseJsonMarshaler.To<T>(o.GuildRaidTicket);
			return true;
		case "GuildRaidTicketRechargeTime":
			v = BaseJsonMarshaler.To<T>(o.GuildRaidTicketRechargeTime);
			return true;
		case "WorldBossTicket":
			v = BaseJsonMarshaler.To<T>(o.WorldBossTicket);
			return true;
		case "WorldBossTicketRechargeTime":
			v = BaseJsonMarshaler.To<T>(o.WorldBossTicketRechargeTime);
			return true;
		case "WorldBossPoint":
			v = BaseJsonMarshaler.To<T>(o.WorldBossPoint);
			return true;
		case "EventDungeonTicket":
			v = BaseJsonMarshaler.To<T>(o.EventDungeonTicket);
			return true;
		case "EventDungeonTicketRechargeTime":
			v = BaseJsonMarshaler.To<T>(o.EventDungeonTicketRechargeTime);
			return true;
		case "EventDungeonPoint":
			v = BaseJsonMarshaler.To<T>(o.EventDungeonPoint);
			return true;
		case "EventDungeonPoint2":
			v = BaseJsonMarshaler.To<T>(o.EventDungeonPoint2);
			return true;
		case "EventDungeonTicketRechargeCount":
			v = BaseJsonMarshaler.To<T>(o.EventDungeonTicketRechargeCount);
			return true;
		case "EventDungeonDailyPoint":
			v = BaseJsonMarshaler.To<T>(o.EventDungeonDailyPoint);
			return true;
		case "EventDungeonDailyPointResetTime":
			v = BaseJsonMarshaler.To<T>(o.EventDungeonDailyPointResetTime);
			return true;
		case "GloryPoint":
			v = BaseJsonMarshaler.To<T>(o.GloryPoint);
			return true;
		case "EventGiftPoint":
			v = BaseJsonMarshaler.To<T>(o.EventGiftPoint);
			return true;
		case "LuaPoint":
			v = BaseJsonMarshaler.To<T>(o.LuaPoint);
			return true;
		case "MazeKey":
			v = BaseJsonMarshaler.To<T>(o.MazeKey);
			return true;
		case "MazeKeyRechargeTime":
			v = BaseJsonMarshaler.To<T>(o.MazeKeyRechargeTime);
			return true;
		case "MazeKeyRechargeCount":
			v = BaseJsonMarshaler.To<T>(o.MazeKeyRechargeCount);
			return true;
		case "CreatedTime":
			v = BaseJsonMarshaler.To<T>(o.CreatedTime);
			return true;
		case "GuildSuppressKey":
			v = BaseJsonMarshaler.To<T>(o.GuildSuppressKey);
			return true;
		case "GuildSuppressKeyRechargeTime":
			v = BaseJsonMarshaler.To<T>(o.GuildSuppressKeyRechargeTime);
			return true;
		case "GuildSuppressKeyRechargeCount":
			v = BaseJsonMarshaler.To<T>(o.GuildSuppressKeyRechargeCount);
			return true;
		case "GuildArenaKey":
			v = BaseJsonMarshaler.To<T>(o.GuildArenaKey);
			return true;
		case "GuildArenaKeyRechargeTime":
			v = BaseJsonMarshaler.To<T>(o.GuildArenaKeyRechargeTime);
			return true;
		case "GuildArenaKeyRechargeCount":
			v = BaseJsonMarshaler.To<T>(o.GuildArenaKeyRechargeCount);
			return true;
		case "TreasureHouseKey":
			v = BaseJsonMarshaler.To<T>(o.TreasureHouseKey);
			return true;
		case "TreasureHouseKeyRechargeTime":
			v = BaseJsonMarshaler.To<T>(o.TreasureHouseKeyRechargeTime);
			return true;
		case "TreasureHouseKeyRechargeCount":
			v = BaseJsonMarshaler.To<T>(o.TreasureHouseKeyRechargeCount);
			return true;
		case "EventWorldBossTicket":
			v = BaseJsonMarshaler.To<T>(o.EventWorldBossTicket);
			return true;
		case "EventWorldBossTicketRechargeTime":
			v = BaseJsonMarshaler.To<T>(o.EventWorldBossTicketRechargeTime);
			return true;
		case "WorldBossGlobalTicket":
			v = BaseJsonMarshaler.To<T>(o.WorldBossGlobalTicket);
			return true;
		case "WorldBossGlobalTicketRechargeTime":
			v = BaseJsonMarshaler.To<T>(o.WorldBossGlobalTicketRechargeTime);
			return true;
		case "GodkingTrialKey":
			v = BaseJsonMarshaler.To<T>(o.GodkingTrialKey);
			return true;
		case "GodkingTrialKeyRechargeTime":
			v = BaseJsonMarshaler.To<T>(o.GodkingTrialKeyRechargeTime);
			return true;
		case "GodkingTrialKeyRechargeCount":
			v = BaseJsonMarshaler.To<T>(o.GodkingTrialKeyRechargeCount);
			return true;
		case "EclipseKey":
			v = BaseJsonMarshaler.To<T>(o.EclipseKey);
			return true;
		case "EclipseKeyRechargeTime":
			v = BaseJsonMarshaler.To<T>(o.EclipseKeyRechargeTime);
			return true;
		case "EclipseKeyRechargeCount":
			v = BaseJsonMarshaler.To<T>(o.EclipseKeyRechargeCount);
			return true;
		case "EventHeroDungeonTicket":
			v = BaseJsonMarshaler.To<T>(o.EventHeroDungeonTicket);
			return true;
		case "EventHeroDungeonTicketRechargeTime":
			v = BaseJsonMarshaler.To<T>(o.EventHeroDungeonTicketRechargeTime);
			return true;
		case "EventDungeonGoldKey":
			v = BaseJsonMarshaler.To<T>(o.EventDungeonGoldKey);
			return true;
		case "EventDungeonGoldKeyRechargeTime":
			v = BaseJsonMarshaler.To<T>(o.EventDungeonGoldKeyRechargeTime);
			return true;
		case "PrisonKey":
			v = BaseJsonMarshaler.To<T>(o.PrisonKey);
			return true;
		case "PrisonKeyRechargeTime":
			v = BaseJsonMarshaler.To<T>(o.PrisonKeyRechargeTime);
			return true;
		case "PrisonKeyRechargeCount":
			v = BaseJsonMarshaler.To<T>(o.PrisonKeyRechargeCount);
			return true;
		case "ShakmehMiddleBossKey":
			v = BaseJsonMarshaler.To<T>(o.ShakmehMiddleBossKey);
			return true;
		case "ShakmehMiddleBossKeyRechargeTime":
			v = BaseJsonMarshaler.To<T>(o.ShakmehMiddleBossKeyRechargeTime);
			return true;
		case "PetFeedKey":
			v = BaseJsonMarshaler.To<T>(o.PetFeedKey);
			return true;
		case "PetFeedRechargeTime":
			v = BaseJsonMarshaler.To<T>(o.PetFeedRechargeTime);
			return true;
		case "PetFeedRechargeCount":
			v = BaseJsonMarshaler.To<T>(o.PetFeedRechargeCount);
			return true;
		case "Sword3":
			v = BaseJsonMarshaler.To<T>(o.Sword3);
			return true;
		case "Sword3RechargeTime":
			v = BaseJsonMarshaler.To<T>(o.Sword3RechargeTime);
			return true;
		case "Sword3RechargeCount":
			v = BaseJsonMarshaler.To<T>(o.Sword3RechargeCount);
			return true;
		case "TechnoEnchantKey":
			v = BaseJsonMarshaler.To<T>(o.TechnoEnchantKey);
			return true;
		case "TechnoEnchantKeyRechargeTime":
			v = BaseJsonMarshaler.To<T>(o.TechnoEnchantKeyRechargeTime);
			return true;
		case "TechnoEnchantKeyRechargeCount":
			v = BaseJsonMarshaler.To<T>(o.TechnoEnchantKeyRechargeCount);
			return true;
		case "EventOrvelPoint":
			v = BaseJsonMarshaler.To<T>(o.EventOrvelPoint);
			return true;
		case "PunishmentRaidKey":
			v = BaseJsonMarshaler.To<T>(o.PunishmentRaidKey);
			return true;
		case "PunishmentRaidKeyRechargeTime":
			v = BaseJsonMarshaler.To<T>(o.PunishmentRaidKeyRechargeTime);
			return true;
		case "PunishmentRaidKeyRechargeCount":
			v = BaseJsonMarshaler.To<T>(o.PunishmentRaidKeyRechargeCount);
			return true;
		default:
			return false;
		}
	}

	public static bool SetField<T>(UserInfo o, string f, T v)
	{
		switch (f)
		{
		case "SessionKey":
			o.SessionKey = BaseJsonMarshaler.To<string>(v);
			return true;
		case "AccountId":
			o.AccountId = BaseJsonMarshaler.To<long>(v);
			return true;
		case "Nick":
			o.Nick = BaseJsonMarshaler.To<string>(v);
			return true;
		case "Gold":
			o.Gold = BaseJsonMarshaler.To<long>(v);
			return true;
		case "Gem":
			o.Gem = BaseJsonMarshaler.To<long>(v);
			return true;
		case "PayGem":
			o.PayGem = BaseJsonMarshaler.To<long>(v);
			return true;
		case "PvpCoin":
			o.PvpCoin = BaseJsonMarshaler.To<long>(v);
			return true;
		case "Stamina":
			o.Stamina = BaseJsonMarshaler.To<long>(v);
			return true;
		case "StaminaRechargeTime":
			o.StaminaRechargeTime = BaseJsonMarshaler.To<string>(v);
			return true;
		case "TeamLevel":
			o.TeamLevel = BaseJsonMarshaler.To<int>(v);
			return true;
		case "TeamExp":
			o.TeamExp = BaseJsonMarshaler.To<int>(v);
			return true;
		case "Sword":
			o.Sword = BaseJsonMarshaler.To<long>(v);
			return true;
		case "SwordRechargeTime":
			o.SwordRechargeTime = BaseJsonMarshaler.To<string>(v);
			return true;
		case "SwordRechargeCount":
			o.SwordRechargeCount = BaseJsonMarshaler.To<int>(v);
			return true;
		case "Sword2":
			o.Sword2 = BaseJsonMarshaler.To<long>(v);
			return true;
		case "Sword2RechargeTime":
			o.Sword2RechargeTime = BaseJsonMarshaler.To<string>(v);
			return true;
		case "AvatarHeroIndex":
			o.AvatarHeroIndex = BaseJsonMarshaler.To<int>(v);
			return true;
		case "RoyalPoint":
			o.RoyalPoint = BaseJsonMarshaler.To<long>(v);
			return true;
		case "UndergroundPrisonKey":
			o.UndergroundPrisonKey = BaseJsonMarshaler.To<long>(v);
			return true;
		case "UndergroundPrisonKeyRechargeTime":
			o.UndergroundPrisonKeyRechargeTime = BaseJsonMarshaler.To<string>(v);
			return true;
		case "UndergroundPrisonKeyRechargeCount":
			o.UndergroundPrisonKeyRechargeCount = BaseJsonMarshaler.To<int>(v);
			return true;
		case "UndergroundLabyrinthKey":
			o.UndergroundLabyrinthKey = BaseJsonMarshaler.To<long>(v);
			return true;
		case "UndergroundLabyrinthKeyRechargeTime":
			o.UndergroundLabyrinthKeyRechargeTime = BaseJsonMarshaler.To<string>(v);
			return true;
		case "Mileage":
			o.Mileage = BaseJsonMarshaler.To<long>(v);
			return true;
		case "FriendshipPoint":
			o.FriendshipPoint = BaseJsonMarshaler.To<long>(v);
			return true;
		case "HideoutKey":
			o.HideoutKey = BaseJsonMarshaler.To<long>(v);
			return true;
		case "HideoutKeyRechargeTime":
			o.HideoutKeyRechargeTime = BaseJsonMarshaler.To<string>(v);
			return true;
		case "HideoutKeyRechargeCount":
			o.HideoutKeyRechargeCount = BaseJsonMarshaler.To<int>(v);
			return true;
		case "ChallengeTowerKey":
			o.ChallengeTowerKey = BaseJsonMarshaler.To<long>(v);
			return true;
		case "ChallengeTowerKeyRechargeTime":
			o.ChallengeTowerKeyRechargeTime = BaseJsonMarshaler.To<string>(v);
			return true;
		case "ChallengeTowerKeyRechargeCount":
			o.ChallengeTowerKeyRechargeCount = BaseJsonMarshaler.To<int>(v);
			return true;
		case "LastChapterIndex":
			o.LastChapterIndex = BaseJsonMarshaler.To<int>(v);
			return true;
		case "LastDungeonIndex":
			o.LastDungeonIndex = BaseJsonMarshaler.To<int>(v);
			return true;
		case "RaidPoint":
			o.RaidPoint = BaseJsonMarshaler.To<long>(v);
			return true;
		case "GuildRaidTicket":
			o.GuildRaidTicket = BaseJsonMarshaler.To<long>(v);
			return true;
		case "GuildRaidTicketRechargeTime":
			o.GuildRaidTicketRechargeTime = BaseJsonMarshaler.To<string>(v);
			return true;
		case "WorldBossTicket":
			o.WorldBossTicket = BaseJsonMarshaler.To<long>(v);
			return true;
		case "WorldBossTicketRechargeTime":
			o.WorldBossTicketRechargeTime = BaseJsonMarshaler.To<string>(v);
			return true;
		case "WorldBossPoint":
			o.WorldBossPoint = BaseJsonMarshaler.To<long>(v);
			return true;
		case "EventDungeonTicket":
			o.EventDungeonTicket = BaseJsonMarshaler.To<long>(v);
			return true;
		case "EventDungeonTicketRechargeTime":
			o.EventDungeonTicketRechargeTime = BaseJsonMarshaler.To<string>(v);
			return true;
		case "EventDungeonPoint":
			o.EventDungeonPoint = BaseJsonMarshaler.To<long>(v);
			return true;
		case "EventDungeonPoint2":
			o.EventDungeonPoint2 = BaseJsonMarshaler.To<long>(v);
			return true;
		case "EventDungeonTicketRechargeCount":
			o.EventDungeonTicketRechargeCount = BaseJsonMarshaler.To<int>(v);
			return true;
		case "EventDungeonDailyPoint":
			o.EventDungeonDailyPoint = BaseJsonMarshaler.To<long>(v);
			return true;
		case "EventDungeonDailyPointResetTime":
			o.EventDungeonDailyPointResetTime = BaseJsonMarshaler.To<string>(v);
			return true;
		case "GloryPoint":
			o.GloryPoint = BaseJsonMarshaler.To<long>(v);
			return true;
		case "EventGiftPoint":
			o.EventGiftPoint = BaseJsonMarshaler.To<long>(v);
			return true;
		case "LuaPoint":
			o.LuaPoint = BaseJsonMarshaler.To<long>(v);
			return true;
		case "MazeKey":
			o.MazeKey = BaseJsonMarshaler.To<long>(v);
			return true;
		case "MazeKeyRechargeTime":
			o.MazeKeyRechargeTime = BaseJsonMarshaler.To<string>(v);
			return true;
		case "MazeKeyRechargeCount":
			o.MazeKeyRechargeCount = BaseJsonMarshaler.To<int>(v);
			return true;
		case "CreatedTime":
			o.CreatedTime = BaseJsonMarshaler.To<string>(v);
			return true;
		case "GuildSuppressKey":
			o.GuildSuppressKey = BaseJsonMarshaler.To<long>(v);
			return true;
		case "GuildSuppressKeyRechargeTime":
			o.GuildSuppressKeyRechargeTime = BaseJsonMarshaler.To<string>(v);
			return true;
		case "GuildSuppressKeyRechargeCount":
			o.GuildSuppressKeyRechargeCount = BaseJsonMarshaler.To<int>(v);
			return true;
		case "GuildArenaKey":
			o.GuildArenaKey = BaseJsonMarshaler.To<long>(v);
			return true;
		case "GuildArenaKeyRechargeTime":
			o.GuildArenaKeyRechargeTime = BaseJsonMarshaler.To<string>(v);
			return true;
		case "GuildArenaKeyRechargeCount":
			o.GuildArenaKeyRechargeCount = BaseJsonMarshaler.To<int>(v);
			return true;
		case "TreasureHouseKey":
			o.TreasureHouseKey = BaseJsonMarshaler.To<long>(v);
			return true;
		case "TreasureHouseKeyRechargeTime":
			o.TreasureHouseKeyRechargeTime = BaseJsonMarshaler.To<string>(v);
			return true;
		case "TreasureHouseKeyRechargeCount":
			o.TreasureHouseKeyRechargeCount = BaseJsonMarshaler.To<int>(v);
			return true;
		case "EventWorldBossTicket":
			o.EventWorldBossTicket = BaseJsonMarshaler.To<long>(v);
			return true;
		case "EventWorldBossTicketRechargeTime":
			o.EventWorldBossTicketRechargeTime = BaseJsonMarshaler.To<string>(v);
			return true;
		case "WorldBossGlobalTicket":
			o.WorldBossGlobalTicket = BaseJsonMarshaler.To<long>(v);
			return true;
		case "WorldBossGlobalTicketRechargeTime":
			o.WorldBossGlobalTicketRechargeTime = BaseJsonMarshaler.To<string>(v);
			return true;
		case "GodkingTrialKey":
			o.GodkingTrialKey = BaseJsonMarshaler.To<long>(v);
			return true;
		case "GodkingTrialKeyRechargeTime":
			o.GodkingTrialKeyRechargeTime = BaseJsonMarshaler.To<string>(v);
			return true;
		case "GodkingTrialKeyRechargeCount":
			o.GodkingTrialKeyRechargeCount = BaseJsonMarshaler.To<int>(v);
			return true;
		case "EclipseKey":
			o.EclipseKey = BaseJsonMarshaler.To<long>(v);
			return true;
		case "EclipseKeyRechargeTime":
			o.EclipseKeyRechargeTime = BaseJsonMarshaler.To<string>(v);
			return true;
		case "EclipseKeyRechargeCount":
			o.EclipseKeyRechargeCount = BaseJsonMarshaler.To<int>(v);
			return true;
		case "EventHeroDungeonTicket":
			o.EventHeroDungeonTicket = BaseJsonMarshaler.To<long>(v);
			return true;
		case "EventHeroDungeonTicketRechargeTime":
			o.EventHeroDungeonTicketRechargeTime = BaseJsonMarshaler.To<string>(v);
			return true;
		case "EventDungeonGoldKey":
			o.EventDungeonGoldKey = BaseJsonMarshaler.To<long>(v);
			return true;
		case "EventDungeonGoldKeyRechargeTime":
			o.EventDungeonGoldKeyRechargeTime = BaseJsonMarshaler.To<string>(v);
			return true;
		case "PrisonKey":
			o.PrisonKey = BaseJsonMarshaler.To<long>(v);
			return true;
		case "PrisonKeyRechargeTime":
			o.PrisonKeyRechargeTime = BaseJsonMarshaler.To<string>(v);
			return true;
		case "PrisonKeyRechargeCount":
			o.PrisonKeyRechargeCount = BaseJsonMarshaler.To<int>(v);
			return true;
		case "ShakmehMiddleBossKey":
			o.ShakmehMiddleBossKey = BaseJsonMarshaler.To<long>(v);
			return true;
		case "ShakmehMiddleBossKeyRechargeTime":
			o.ShakmehMiddleBossKeyRechargeTime = BaseJsonMarshaler.To<string>(v);
			return true;
		case "PetFeedKey":
			o.PetFeedKey = BaseJsonMarshaler.To<long>(v);
			return true;
		case "PetFeedRechargeTime":
			o.PetFeedRechargeTime = BaseJsonMarshaler.To<string>(v);
			return true;
		case "PetFeedRechargeCount":
			o.PetFeedRechargeCount = BaseJsonMarshaler.To<int>(v);
			return true;
		case "Sword3":
			o.Sword3 = BaseJsonMarshaler.To<long>(v);
			return true;
		case "Sword3RechargeTime":
			o.Sword3RechargeTime = BaseJsonMarshaler.To<string>(v);
			return true;
		case "Sword3RechargeCount":
			o.Sword3RechargeCount = BaseJsonMarshaler.To<int>(v);
			return true;
		case "TechnoEnchantKey":
			o.TechnoEnchantKey = BaseJsonMarshaler.To<long>(v);
			return true;
		case "TechnoEnchantKeyRechargeTime":
			o.TechnoEnchantKeyRechargeTime = BaseJsonMarshaler.To<string>(v);
			return true;
		case "TechnoEnchantKeyRechargeCount":
			o.TechnoEnchantKeyRechargeCount = BaseJsonMarshaler.To<int>(v);
			return true;
		case "EventOrvelPoint":
			o.EventOrvelPoint = BaseJsonMarshaler.To<long>(v);
			return true;
		case "PunishmentRaidKey":
			o.PunishmentRaidKey = BaseJsonMarshaler.To<long>(v);
			return true;
		case "PunishmentRaidKeyRechargeTime":
			o.PunishmentRaidKeyRechargeTime = BaseJsonMarshaler.To<string>(v);
			return true;
		case "PunishmentRaidKeyRechargeCount":
			o.PunishmentRaidKeyRechargeCount = BaseJsonMarshaler.To<int>(v);
			return true;
		default:
			return false;
		}
	}

	public static string EncodeJsonString(UserInfo o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(UserInfo o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		if (o.SessionKey != null)
		{
			dictionary.Add("SessionKey", o.SessionKey);
		}
		dictionary.Add("AccountId", o.AccountId.ToString());
		if (o.Nick != null)
		{
			dictionary.Add("Nick", o.Nick);
		}
		dictionary.Add("Gold", o.Gold.ToString());
		dictionary.Add("Gem", o.Gem.ToString());
		dictionary.Add("PayGem", o.PayGem.ToString());
		dictionary.Add("PvpCoin", o.PvpCoin.ToString());
		dictionary.Add("Stamina", o.Stamina.ToString());
		if (o.StaminaRechargeTime != null)
		{
			dictionary.Add("StaminaRechargeTime", o.StaminaRechargeTime);
		}
		int teamLevel = o.TeamLevel;
		dictionary.Add("TeamLevel", ((long)teamLevel).ToString());
		int teamExp = o.TeamExp;
		dictionary.Add("TeamExp", ((long)teamExp).ToString());
		dictionary.Add("Sword", o.Sword.ToString());
		if (o.SwordRechargeTime != null)
		{
			dictionary.Add("SwordRechargeTime", o.SwordRechargeTime);
		}
		int swordRechargeCount = o.SwordRechargeCount;
		dictionary.Add("SwordRechargeCount", ((long)swordRechargeCount).ToString());
		dictionary.Add("Sword2", o.Sword2.ToString());
		if (o.Sword2RechargeTime != null)
		{
			dictionary.Add("Sword2RechargeTime", o.Sword2RechargeTime);
		}
		int avatarHeroIndex = o.AvatarHeroIndex;
		dictionary.Add("AvatarHeroIndex", ((long)avatarHeroIndex).ToString());
		dictionary.Add("RoyalPoint", o.RoyalPoint.ToString());
		dictionary.Add("UndergroundPrisonKey", o.UndergroundPrisonKey.ToString());
		if (o.UndergroundPrisonKeyRechargeTime != null)
		{
			dictionary.Add("UndergroundPrisonKeyRechargeTime", o.UndergroundPrisonKeyRechargeTime);
		}
		int undergroundPrisonKeyRechargeCount = o.UndergroundPrisonKeyRechargeCount;
		dictionary.Add("UndergroundPrisonKeyRechargeCount", ((long)undergroundPrisonKeyRechargeCount).ToString());
		dictionary.Add("UndergroundLabyrinthKey", o.UndergroundLabyrinthKey.ToString());
		if (o.UndergroundLabyrinthKeyRechargeTime != null)
		{
			dictionary.Add("UndergroundLabyrinthKeyRechargeTime", o.UndergroundLabyrinthKeyRechargeTime);
		}
		dictionary.Add("Mileage", o.Mileage.ToString());
		dictionary.Add("FriendshipPoint", o.FriendshipPoint.ToString());
		dictionary.Add("HideoutKey", o.HideoutKey.ToString());
		if (o.HideoutKeyRechargeTime != null)
		{
			dictionary.Add("HideoutKeyRechargeTime", o.HideoutKeyRechargeTime);
		}
		int hideoutKeyRechargeCount = o.HideoutKeyRechargeCount;
		dictionary.Add("HideoutKeyRechargeCount", ((long)hideoutKeyRechargeCount).ToString());
		dictionary.Add("ChallengeTowerKey", o.ChallengeTowerKey.ToString());
		if (o.ChallengeTowerKeyRechargeTime != null)
		{
			dictionary.Add("ChallengeTowerKeyRechargeTime", o.ChallengeTowerKeyRechargeTime);
		}
		int challengeTowerKeyRechargeCount = o.ChallengeTowerKeyRechargeCount;
		dictionary.Add("ChallengeTowerKeyRechargeCount", ((long)challengeTowerKeyRechargeCount).ToString());
		int lastChapterIndex = o.LastChapterIndex;
		dictionary.Add("LastChapterIndex", ((long)lastChapterIndex).ToString());
		int lastDungeonIndex = o.LastDungeonIndex;
		dictionary.Add("LastDungeonIndex", ((long)lastDungeonIndex).ToString());
		dictionary.Add("RaidPoint", o.RaidPoint.ToString());
		dictionary.Add("GuildRaidTicket", o.GuildRaidTicket.ToString());
		if (o.GuildRaidTicketRechargeTime != null)
		{
			dictionary.Add("GuildRaidTicketRechargeTime", o.GuildRaidTicketRechargeTime);
		}
		dictionary.Add("WorldBossTicket", o.WorldBossTicket.ToString());
		if (o.WorldBossTicketRechargeTime != null)
		{
			dictionary.Add("WorldBossTicketRechargeTime", o.WorldBossTicketRechargeTime);
		}
		dictionary.Add("WorldBossPoint", o.WorldBossPoint.ToString());
		dictionary.Add("EventDungeonTicket", o.EventDungeonTicket.ToString());
		if (o.EventDungeonTicketRechargeTime != null)
		{
			dictionary.Add("EventDungeonTicketRechargeTime", o.EventDungeonTicketRechargeTime);
		}
		dictionary.Add("EventDungeonPoint", o.EventDungeonPoint.ToString());
		dictionary.Add("EventDungeonPoint2", o.EventDungeonPoint2.ToString());
		int eventDungeonTicketRechargeCount = o.EventDungeonTicketRechargeCount;
		dictionary.Add("EventDungeonTicketRechargeCount", ((long)eventDungeonTicketRechargeCount).ToString());
		dictionary.Add("EventDungeonDailyPoint", o.EventDungeonDailyPoint.ToString());
		if (o.EventDungeonDailyPointResetTime != null)
		{
			dictionary.Add("EventDungeonDailyPointResetTime", o.EventDungeonDailyPointResetTime);
		}
		dictionary.Add("GloryPoint", o.GloryPoint.ToString());
		dictionary.Add("EventGiftPoint", o.EventGiftPoint.ToString());
		dictionary.Add("LuaPoint", o.LuaPoint.ToString());
		dictionary.Add("MazeKey", o.MazeKey.ToString());
		if (o.MazeKeyRechargeTime != null)
		{
			dictionary.Add("MazeKeyRechargeTime", o.MazeKeyRechargeTime);
		}
		int mazeKeyRechargeCount = o.MazeKeyRechargeCount;
		dictionary.Add("MazeKeyRechargeCount", ((long)mazeKeyRechargeCount).ToString());
		if (o.CreatedTime != null)
		{
			dictionary.Add("CreatedTime", o.CreatedTime);
		}
		dictionary.Add("GuildSuppressKey", o.GuildSuppressKey.ToString());
		if (o.GuildSuppressKeyRechargeTime != null)
		{
			dictionary.Add("GuildSuppressKeyRechargeTime", o.GuildSuppressKeyRechargeTime);
		}
		int guildSuppressKeyRechargeCount = o.GuildSuppressKeyRechargeCount;
		dictionary.Add("GuildSuppressKeyRechargeCount", ((long)guildSuppressKeyRechargeCount).ToString());
		dictionary.Add("GuildArenaKey", o.GuildArenaKey.ToString());
		if (o.GuildArenaKeyRechargeTime != null)
		{
			dictionary.Add("GuildArenaKeyRechargeTime", o.GuildArenaKeyRechargeTime);
		}
		int guildArenaKeyRechargeCount = o.GuildArenaKeyRechargeCount;
		dictionary.Add("GuildArenaKeyRechargeCount", ((long)guildArenaKeyRechargeCount).ToString());
		dictionary.Add("TreasureHouseKey", o.TreasureHouseKey.ToString());
		if (o.TreasureHouseKeyRechargeTime != null)
		{
			dictionary.Add("TreasureHouseKeyRechargeTime", o.TreasureHouseKeyRechargeTime);
		}
		int treasureHouseKeyRechargeCount = o.TreasureHouseKeyRechargeCount;
		dictionary.Add("TreasureHouseKeyRechargeCount", ((long)treasureHouseKeyRechargeCount).ToString());
		dictionary.Add("EventWorldBossTicket", o.EventWorldBossTicket.ToString());
		if (o.EventWorldBossTicketRechargeTime != null)
		{
			dictionary.Add("EventWorldBossTicketRechargeTime", o.EventWorldBossTicketRechargeTime);
		}
		dictionary.Add("WorldBossGlobalTicket", o.WorldBossGlobalTicket.ToString());
		if (o.WorldBossGlobalTicketRechargeTime != null)
		{
			dictionary.Add("WorldBossGlobalTicketRechargeTime", o.WorldBossGlobalTicketRechargeTime);
		}
		dictionary.Add("GodkingTrialKey", o.GodkingTrialKey.ToString());
		if (o.GodkingTrialKeyRechargeTime != null)
		{
			dictionary.Add("GodkingTrialKeyRechargeTime", o.GodkingTrialKeyRechargeTime);
		}
		int godkingTrialKeyRechargeCount = o.GodkingTrialKeyRechargeCount;
		dictionary.Add("GodkingTrialKeyRechargeCount", ((long)godkingTrialKeyRechargeCount).ToString());
		dictionary.Add("EclipseKey", o.EclipseKey.ToString());
		if (o.EclipseKeyRechargeTime != null)
		{
			dictionary.Add("EclipseKeyRechargeTime", o.EclipseKeyRechargeTime);
		}
		int eclipseKeyRechargeCount = o.EclipseKeyRechargeCount;
		dictionary.Add("EclipseKeyRechargeCount", ((long)eclipseKeyRechargeCount).ToString());
		dictionary.Add("EventHeroDungeonTicket", o.EventHeroDungeonTicket.ToString());
		if (o.EventHeroDungeonTicketRechargeTime != null)
		{
			dictionary.Add("EventHeroDungeonTicketRechargeTime", o.EventHeroDungeonTicketRechargeTime);
		}
		dictionary.Add("EventDungeonGoldKey", o.EventDungeonGoldKey.ToString());
		if (o.EventDungeonGoldKeyRechargeTime != null)
		{
			dictionary.Add("EventDungeonGoldKeyRechargeTime", o.EventDungeonGoldKeyRechargeTime);
		}
		dictionary.Add("PrisonKey", o.PrisonKey.ToString());
		if (o.PrisonKeyRechargeTime != null)
		{
			dictionary.Add("PrisonKeyRechargeTime", o.PrisonKeyRechargeTime);
		}
		int prisonKeyRechargeCount = o.PrisonKeyRechargeCount;
		dictionary.Add("PrisonKeyRechargeCount", ((long)prisonKeyRechargeCount).ToString());
		dictionary.Add("ShakmehMiddleBossKey", o.ShakmehMiddleBossKey.ToString());
		if (o.ShakmehMiddleBossKeyRechargeTime != null)
		{
			dictionary.Add("ShakmehMiddleBossKeyRechargeTime", o.ShakmehMiddleBossKeyRechargeTime);
		}
		dictionary.Add("PetFeedKey", o.PetFeedKey.ToString());
		if (o.PetFeedRechargeTime != null)
		{
			dictionary.Add("PetFeedRechargeTime", o.PetFeedRechargeTime);
		}
		int petFeedRechargeCount = o.PetFeedRechargeCount;
		dictionary.Add("PetFeedRechargeCount", ((long)petFeedRechargeCount).ToString());
		dictionary.Add("Sword3", o.Sword3.ToString());
		if (o.Sword3RechargeTime != null)
		{
			dictionary.Add("Sword3RechargeTime", o.Sword3RechargeTime);
		}
		int sword3RechargeCount = o.Sword3RechargeCount;
		dictionary.Add("Sword3RechargeCount", ((long)sword3RechargeCount).ToString());
		dictionary.Add("TechnoEnchantKey", o.TechnoEnchantKey.ToString());
		if (o.TechnoEnchantKeyRechargeTime != null)
		{
			dictionary.Add("TechnoEnchantKeyRechargeTime", o.TechnoEnchantKeyRechargeTime);
		}
		int technoEnchantKeyRechargeCount = o.TechnoEnchantKeyRechargeCount;
		dictionary.Add("TechnoEnchantKeyRechargeCount", ((long)technoEnchantKeyRechargeCount).ToString());
		dictionary.Add("EventOrvelPoint", o.EventOrvelPoint.ToString());
		dictionary.Add("PunishmentRaidKey", o.PunishmentRaidKey.ToString());
		if (o.PunishmentRaidKeyRechargeTime != null)
		{
			dictionary.Add("PunishmentRaidKeyRechargeTime", o.PunishmentRaidKeyRechargeTime);
		}
		int punishmentRaidKeyRechargeCount = o.PunishmentRaidKeyRechargeCount;
		dictionary.Add("PunishmentRaidKeyRechargeCount", ((long)punishmentRaidKeyRechargeCount).ToString());
		return dictionary;
	}
}
