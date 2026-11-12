using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_PlayerMiscInfo
{
	public static PlayerMiscInfo Parse(string s)
	{
		return Parse(s, new PlayerMiscInfo());
	}

	public static PlayerMiscInfo Parse(string s, PlayerMiscInfo o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static PlayerMiscInfo Parse(IDictionary d)
	{
		return Parse(d, new PlayerMiscInfo());
	}

	public static PlayerMiscInfo Parse(IDictionary d, PlayerMiscInfo o)
	{
		o.InventoryExtend = BaseJsonMarshaler.ParseValueInt(d, "InventoryExtend");
		o.EquipStorageSlotExtend = BaseJsonMarshaler.ParseValueInt(d, "EquipStorageSlotExtend");
		o.ChestExtend = BaseJsonMarshaler.ParseValueInt(d, "ChestExtend");
		o.PunishmentRuneStorageSlotExtend = BaseJsonMarshaler.ParseValueInt(d, "PunishmentRuneStorageSlotExtend");
		o.DailyQuestResetTime = BaseJsonMarshaler.ParseValueString(d, "DailyQuestResetTime");
		o.WeeklyQuestResetTime = BaseJsonMarshaler.ParseValueString(d, "WeeklyQuestResetTime");
		o.DOWResetTime = BaseJsonMarshaler.ParseValueString(d, "DOWResetTime");
		o.UTCOffsetHour = BaseJsonMarshaler.ParseValueInt(d, "UTCOffsetHour");
		o.HeroShopSlotExtend = BaseJsonMarshaler.ParseValueInt(d, "HeroShopSlotExtend");
		o.LastGlobalMailIndex = BaseJsonMarshaler.ParseValueLong(d, "LastGlobalMailIndex");
		o.QuestDailyRepeatResetTime = BaseJsonMarshaler.ParseValueString(d, "QuestDailyRepeatResetTime");
		o.MainQuestStep = BaseJsonMarshaler.ParseValueInt(d, "MainQuestStep");
		o.MainQuestProgress = BaseJsonMarshaler.ParseValueInt(d, "MainQuestProgress");
		o.ScenarioQuestIndex = BaseJsonMarshaler.ParseValueInt(d, "ScenarioQuestIndex");
		o.ScenarioQuestProgress = BaseJsonMarshaler.ParseValueInt(d, "ScenarioQuestProgress");
		o.LastRaidResetTime = BaseJsonMarshaler.ParseValueString(d, "LastRaidResetTime");
		o.GuildWithdrawTime = BaseJsonMarshaler.ParseValueString(d, "GuildWithdrawTime");
		o.GuildWithdrawCount = BaseJsonMarshaler.ParseValueInt(d, "GuildWithdrawCount");
		o.DailyAccFriendshipPoint = BaseJsonMarshaler.ParseValueLong(d, "DailyAccFriendshipPoint");
		o.DailyAccFriendshipPointResetTime = BaseJsonMarshaler.ParseValueString(d, "DailyAccFriendshipPointResetTime");
		o.RenewOptionEquipItemSlotIndex = BaseJsonMarshaler.ParseValueInt(d, "RenewOptionEquipItemSlotIndex");
		o.RenewOptionResultIndex = BaseJsonMarshaler.ParseValueInt(d, "RenewOptionResultIndex");
		o.RenewOptionResultStep = BaseJsonMarshaler.ParseValueInt(d, "RenewOptionResultStep");
		o.RenewOptionSlotIndex = BaseJsonMarshaler.ParseValueInt(d, "RenewOptionSlotIndex");
		o.RenewEnchantOptionEquipItemSlotIndex = BaseJsonMarshaler.ParseValueInt(d, "RenewEnchantOptionEquipItemSlotIndex");
		o.RenewEnchantOptionResultIndex = BaseJsonMarshaler.ParseValueInt(d, "RenewEnchantOptionResultIndex");
		o.RenewEnchantOptionResultStep = BaseJsonMarshaler.ParseValueInt(d, "RenewEnchantOptionResultStep");
		o.LastEventChapterIndex = BaseJsonMarshaler.ParseValueInt(d, "LastEventChapterIndex");
		o.LastEventDungeonIndex = BaseJsonMarshaler.ParseValueInt(d, "LastEventDungeonIndex");
		o.EventWorldBossCompletedTime = BaseJsonMarshaler.ParseValueString(d, "EventWorldBossCompletedTime");
		o.LoginDailyCount = BaseJsonMarshaler.ParseValueInt(d, "LoginDailyCount");
		o.GuildArenaSessionInfo = BaseJsonMarshaler.ParseObject(d, "GuildArenaSessionInfo", JM_NShared_GuildArenaSessionInfo.Parse);
		o.LoginDailyUpdatedTime = BaseJsonMarshaler.ParseValueString(d, "LoginDailyUpdatedTime");
		o.SuccessiveDayCount = BaseJsonMarshaler.ParseValueInt(d, "SuccessiveDayCount");
		o.MaxSuccessiveDayCount = BaseJsonMarshaler.ParseValueInt(d, "MaxSuccessiveDayCount");
		o.PlayTimeUpdatedTime = BaseJsonMarshaler.ParseValueString(d, "PlayTimeUpdatedTime");
		o.IsArchiveUpdate = BaseJsonMarshaler.ParseValueInt(d, "IsArchiveUpdate");
		o.NickChange = BaseJsonMarshaler.ParseValueInt(d, "NickChange");
		o.AppReviewPopup = BaseJsonMarshaler.ParseValueInt(d, "AppReviewPopup");
		o.MazeTowerRewardStep = BaseJsonMarshaler.ParseValueInt(d, "MazeTowerRewardStep");
		o.MazeTowerRewardedTime = BaseJsonMarshaler.ParseValueString(d, "MazeTowerRewardedTime");
		o.OpenGuildTime = BaseJsonMarshaler.ParseValueString(d, "OpenGuildTime");
		o.GuildMemberRewardedTime = BaseJsonMarshaler.ParseValueString(d, "GuildMemberRewardedTime");
		o.ChapterRenewedTime = BaseJsonMarshaler.ParseValueString(d, "ChapterRenewedTime");
		o.MainQuestRenewedTime = BaseJsonMarshaler.ParseValueString(d, "MainQuestRenewedTime");
		o.SubQuestRenewedTime = BaseJsonMarshaler.ParseValueString(d, "SubQuestRenewedTime");
		o.SoulStoneMileage = BaseJsonMarshaler.ParseValueLong(d, "SoulStoneMileage");
		o.RestoreSoulStoneIndices = BaseJsonMarshaler.ParseValueString(d, "RestoreSoulStoneIndices");
		o.HeroShopItemListIndex = BaseJsonMarshaler.ParseValueInt(d, "HeroShopItemListIndex");
		o.CountryCode = BaseJsonMarshaler.ParseValueString(d, "CountryCode");
		o.NewbiePopUpEndTime = BaseJsonMarshaler.ParseValueString(d, "NewbiePopUpEndTime");
		o.NewbiePopUpCompleteTime = BaseJsonMarshaler.ParseValueString(d, "NewbiePopUpCompleteTime");
		o.ShakemehPassiveInfos = BaseJsonMarshaler.ParseArrayCast(d, "ShakemehPassiveInfos", BaseJsonMarshaler.CastToInt);
		o.ValanceRenewOptionInfo = BaseJsonMarshaler.ParseObject(d, "ValanceRenewOptionInfo", JM_NShared_ValanceRenewOptionInfo.Parse);
		o.ValanceEnchantOptionInfo = BaseJsonMarshaler.ParseObject(d, "ValanceEnchantOptionInfo", JM_NShared_ValanceEnchantOptionInfo.Parse);
		o.ValanceRenewSkillInfo = BaseJsonMarshaler.ParseObject(d, "ValanceRenewSkillInfo", JM_NShared_ValanceRenewSkillInfo.Parse);
		return o;
	}

	public static string EncodeJsonString(PlayerMiscInfo o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(PlayerMiscInfo o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		int inventoryExtend = o.InventoryExtend;
		dictionary.Add("InventoryExtend", ((long)inventoryExtend).ToString());
		int equipStorageSlotExtend = o.EquipStorageSlotExtend;
		dictionary.Add("EquipStorageSlotExtend", ((long)equipStorageSlotExtend).ToString());
		int chestExtend = o.ChestExtend;
		dictionary.Add("ChestExtend", ((long)chestExtend).ToString());
		int punishmentRuneStorageSlotExtend = o.PunishmentRuneStorageSlotExtend;
		dictionary.Add("PunishmentRuneStorageSlotExtend", ((long)punishmentRuneStorageSlotExtend).ToString());
		if (o.DailyQuestResetTime != null)
		{
			dictionary.Add("DailyQuestResetTime", o.DailyQuestResetTime);
		}
		if (o.WeeklyQuestResetTime != null)
		{
			dictionary.Add("WeeklyQuestResetTime", o.WeeklyQuestResetTime);
		}
		if (o.DOWResetTime != null)
		{
			dictionary.Add("DOWResetTime", o.DOWResetTime);
		}
		int uTCOffsetHour = o.UTCOffsetHour;
		dictionary.Add("UTCOffsetHour", ((long)uTCOffsetHour).ToString());
		int heroShopSlotExtend = o.HeroShopSlotExtend;
		dictionary.Add("HeroShopSlotExtend", ((long)heroShopSlotExtend).ToString());
		dictionary.Add("LastGlobalMailIndex", o.LastGlobalMailIndex.ToString());
		if (o.QuestDailyRepeatResetTime != null)
		{
			dictionary.Add("QuestDailyRepeatResetTime", o.QuestDailyRepeatResetTime);
		}
		int mainQuestStep = o.MainQuestStep;
		dictionary.Add("MainQuestStep", ((long)mainQuestStep).ToString());
		int mainQuestProgress = o.MainQuestProgress;
		dictionary.Add("MainQuestProgress", ((long)mainQuestProgress).ToString());
		int scenarioQuestIndex = o.ScenarioQuestIndex;
		dictionary.Add("ScenarioQuestIndex", ((long)scenarioQuestIndex).ToString());
		int scenarioQuestProgress = o.ScenarioQuestProgress;
		dictionary.Add("ScenarioQuestProgress", ((long)scenarioQuestProgress).ToString());
		if (o.LastRaidResetTime != null)
		{
			dictionary.Add("LastRaidResetTime", o.LastRaidResetTime);
		}
		if (o.GuildWithdrawTime != null)
		{
			dictionary.Add("GuildWithdrawTime", o.GuildWithdrawTime);
		}
		int guildWithdrawCount = o.GuildWithdrawCount;
		dictionary.Add("GuildWithdrawCount", ((long)guildWithdrawCount).ToString());
		dictionary.Add("DailyAccFriendshipPoint", o.DailyAccFriendshipPoint.ToString());
		if (o.DailyAccFriendshipPointResetTime != null)
		{
			dictionary.Add("DailyAccFriendshipPointResetTime", o.DailyAccFriendshipPointResetTime);
		}
		int renewOptionEquipItemSlotIndex = o.RenewOptionEquipItemSlotIndex;
		dictionary.Add("RenewOptionEquipItemSlotIndex", ((long)renewOptionEquipItemSlotIndex).ToString());
		int renewOptionResultIndex = o.RenewOptionResultIndex;
		dictionary.Add("RenewOptionResultIndex", ((long)renewOptionResultIndex).ToString());
		int renewOptionResultStep = o.RenewOptionResultStep;
		dictionary.Add("RenewOptionResultStep", ((long)renewOptionResultStep).ToString());
		int renewOptionSlotIndex = o.RenewOptionSlotIndex;
		dictionary.Add("RenewOptionSlotIndex", ((long)renewOptionSlotIndex).ToString());
		int renewEnchantOptionEquipItemSlotIndex = o.RenewEnchantOptionEquipItemSlotIndex;
		dictionary.Add("RenewEnchantOptionEquipItemSlotIndex", ((long)renewEnchantOptionEquipItemSlotIndex).ToString());
		int renewEnchantOptionResultIndex = o.RenewEnchantOptionResultIndex;
		dictionary.Add("RenewEnchantOptionResultIndex", ((long)renewEnchantOptionResultIndex).ToString());
		int renewEnchantOptionResultStep = o.RenewEnchantOptionResultStep;
		dictionary.Add("RenewEnchantOptionResultStep", ((long)renewEnchantOptionResultStep).ToString());
		int lastEventChapterIndex = o.LastEventChapterIndex;
		dictionary.Add("LastEventChapterIndex", ((long)lastEventChapterIndex).ToString());
		int lastEventDungeonIndex = o.LastEventDungeonIndex;
		dictionary.Add("LastEventDungeonIndex", ((long)lastEventDungeonIndex).ToString());
		if (o.EventWorldBossCompletedTime != null)
		{
			dictionary.Add("EventWorldBossCompletedTime", o.EventWorldBossCompletedTime);
		}
		int loginDailyCount = o.LoginDailyCount;
		dictionary.Add("LoginDailyCount", ((long)loginDailyCount).ToString());
		dictionary.Add("GuildArenaSessionInfo", JM_NShared_GuildArenaSessionInfo.EncodeJsonObject(o.GuildArenaSessionInfo));
		if (o.LoginDailyUpdatedTime != null)
		{
			dictionary.Add("LoginDailyUpdatedTime", o.LoginDailyUpdatedTime);
		}
		int successiveDayCount = o.SuccessiveDayCount;
		dictionary.Add("SuccessiveDayCount", ((long)successiveDayCount).ToString());
		int maxSuccessiveDayCount = o.MaxSuccessiveDayCount;
		dictionary.Add("MaxSuccessiveDayCount", ((long)maxSuccessiveDayCount).ToString());
		if (o.PlayTimeUpdatedTime != null)
		{
			dictionary.Add("PlayTimeUpdatedTime", o.PlayTimeUpdatedTime);
		}
		int isArchiveUpdate = o.IsArchiveUpdate;
		dictionary.Add("IsArchiveUpdate", ((long)isArchiveUpdate).ToString());
		int nickChange = o.NickChange;
		dictionary.Add("NickChange", ((long)nickChange).ToString());
		int appReviewPopup = o.AppReviewPopup;
		dictionary.Add("AppReviewPopup", ((long)appReviewPopup).ToString());
		int mazeTowerRewardStep = o.MazeTowerRewardStep;
		dictionary.Add("MazeTowerRewardStep", ((long)mazeTowerRewardStep).ToString());
		if (o.MazeTowerRewardedTime != null)
		{
			dictionary.Add("MazeTowerRewardedTime", o.MazeTowerRewardedTime);
		}
		if (o.OpenGuildTime != null)
		{
			dictionary.Add("OpenGuildTime", o.OpenGuildTime);
		}
		if (o.GuildMemberRewardedTime != null)
		{
			dictionary.Add("GuildMemberRewardedTime", o.GuildMemberRewardedTime);
		}
		if (o.ChapterRenewedTime != null)
		{
			dictionary.Add("ChapterRenewedTime", o.ChapterRenewedTime);
		}
		if (o.MainQuestRenewedTime != null)
		{
			dictionary.Add("MainQuestRenewedTime", o.MainQuestRenewedTime);
		}
		if (o.SubQuestRenewedTime != null)
		{
			dictionary.Add("SubQuestRenewedTime", o.SubQuestRenewedTime);
		}
		dictionary.Add("SoulStoneMileage", o.SoulStoneMileage.ToString());
		if (o.RestoreSoulStoneIndices != null)
		{
			dictionary.Add("RestoreSoulStoneIndices", o.RestoreSoulStoneIndices);
		}
		int heroShopItemListIndex = o.HeroShopItemListIndex;
		dictionary.Add("HeroShopItemListIndex", ((long)heroShopItemListIndex).ToString());
		if (o.CountryCode != null)
		{
			dictionary.Add("CountryCode", o.CountryCode);
		}
		if (o.NewbiePopUpEndTime != null)
		{
			dictionary.Add("NewbiePopUpEndTime", o.NewbiePopUpEndTime);
		}
		if (o.NewbiePopUpCompleteTime != null)
		{
			dictionary.Add("NewbiePopUpCompleteTime", o.NewbiePopUpCompleteTime);
		}
		dictionary.Add("ShakemehPassiveInfos", BaseJsonMarshaler.EncodeArray(o.ShakemehPassiveInfos));
		dictionary.Add("ValanceRenewOptionInfo", JM_NShared_ValanceRenewOptionInfo.EncodeJsonObject(o.ValanceRenewOptionInfo));
		dictionary.Add("ValanceEnchantOptionInfo", JM_NShared_ValanceEnchantOptionInfo.EncodeJsonObject(o.ValanceEnchantOptionInfo));
		dictionary.Add("ValanceRenewSkillInfo", JM_NShared_ValanceRenewSkillInfo.EncodeJsonObject(o.ValanceRenewSkillInfo));
		return dictionary;
	}
}
