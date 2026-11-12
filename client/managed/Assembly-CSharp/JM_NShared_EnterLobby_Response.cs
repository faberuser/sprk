using System.Collections;
using MiniJSON;
using NShared;
using NShared.EnterLobby;

public static class JM_NShared_EnterLobby_Response
{
	public static NShared.EnterLobby.Response Parse(string s)
	{
		return Parse(s, new NShared.EnterLobby.Response());
	}

	public static NShared.EnterLobby.Response Parse(string s, NShared.EnterLobby.Response o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.EnterLobby.Response Parse(IDictionary d)
	{
		return Parse(d, new NShared.EnterLobby.Response());
	}

	public static NShared.EnterLobby.Response Parse(IDictionary d, NShared.EnterLobby.Response o)
	{
		o.ForwardHost = BaseJsonMarshaler.ParseValueString(d, "ForwardHost");
		o.ServerLocalTime = BaseJsonMarshaler.ParseValueString(d, "ServerLocalTime");
		o.ServerUTCTime = BaseJsonMarshaler.ParseValueString(d, "ServerUTCTime");
		o.ServerUTCOffsetHour = BaseJsonMarshaler.ParseValueInt(d, "ServerUTCOffsetHour");
		o.DungeonInfos = BaseJsonMarshaler.ParseArray(d, "DungeonInfos", JM_NShared_ChapterDungeonInfo.Parse);
		o.HideoutDungeonInfos = BaseJsonMarshaler.ParseArray(d, "HideoutDungeonInfos", JM_NShared_HideoutDungeonInfo.Parse);
		o.ConquestDungeonInfos = BaseJsonMarshaler.ParseArray(d, "ConquestDungeonInfos", JM_NShared_ConquestDungeonInfo.Parse);
		o.TowerInfos = BaseJsonMarshaler.ParseArray(d, "TowerInfos", JM_NShared_TowerInfo.Parse);
		o.AttendanceDatas = BaseJsonMarshaler.ParseArray(d, "AttendanceDatas", JM_NShared_AttendanceData.Parse);
		o.LobbyNotices = BaseJsonMarshaler.ParseArray(d, "LobbyNotices", JM_NShared_LobbyNotice.Parse);
		o.CurStamina = BaseJsonMarshaler.ParseValueInt(d, "CurStamina");
		o.StaminaRechargeTime = BaseJsonMarshaler.ParseValueString(d, "StaminaRechargeTime");
		o.ContentsStatuses = BaseJsonMarshaler.ParseArray(d, "ContentsStatuses", JM_NShared_ContentsStatusInfo.Parse);
		o.ContentsValues = BaseJsonMarshaler.ParseArray(d, "ContentsValues", JM_NShared_ContentsValueInfo.Parse);
		o.CraftSlotResults = BaseJsonMarshaler.ParseArray(d, "CraftSlotResults", JM_NShared_CraftSlotResultInfo.Parse);
		o.DevParam = BaseJsonMarshaler.ParseValueString(d, "DevParam");
		o.DayOfYear = BaseJsonMarshaler.ParseValueInt(d, "DayOfYear");
		o.WeekOfYear = BaseJsonMarshaler.ParseValueInt(d, "WeekOfYear");
		o.MonthOfYear = BaseJsonMarshaler.ParseValueInt(d, "MonthOfYear");
		o.MonthFromMillennium = BaseJsonMarshaler.ParseValueInt(d, "MonthFromMillennium");
		o.RemainDaySecond = BaseJsonMarshaler.ParseValueInt(d, "RemainDaySecond");
		o.RemainWeekSecond = BaseJsonMarshaler.ParseValueInt(d, "RemainWeekSecond");
		o.RemainMonthSecond = BaseJsonMarshaler.ParseValueInt(d, "RemainMonthSecond");
		o.DoyRandomSeed = BaseJsonMarshaler.ParseValueInt(d, "DoyRandomSeed");
		o.WoyRandomSeed = BaseJsonMarshaler.ParseValueInt(d, "WoyRandomSeed");
		o.EventCalendarVersion = BaseJsonMarshaler.ParseValueInt(d, "EventCalendarVersion");
		o.EventDungeonPointResult = BaseJsonMarshaler.ParseObject(d, "EventDungeonPointResult", JM_NShared_EventDungeonPointResultInfo.Parse);
		o.LoginDailyCount = BaseJsonMarshaler.ParseValueInt(d, "LoginDailyCount");
		o.GuildArenaSessionInfo = BaseJsonMarshaler.ParseArray(d, "GuildArenaSessionInfo", JM_NShared_GuildArenaSessionInfo.Parse);
		o.PushRewardInfo = BaseJsonMarshaler.ParseObject(d, "PushRewardInfo", JM_NShared_PushRewardInfo.Parse);
		o.OpendMissionCategories = BaseJsonMarshaler.ParseArray(d, "OpendMissionCategories", JM_NShared_MissionCategoryInfo.Parse);
		o.WebEvent = BaseJsonMarshaler.ParseObject(d, "WebEvent", JM_NShared_WebEventInfo.Parse);
		o.GuildSuppressSessionInfo = BaseJsonMarshaler.ParseObject(d, "GuildSuppressSessionInfo", JM_NShared_GuildSuppressSessionInfo.Parse);
		o.PayShopProductInfos = BaseJsonMarshaler.ParseArray(d, "PayShopProductInfos", JM_NShared_NewPayShopProductInfo.Parse);
		o.PlayerProductPurchaseInfos = BaseJsonMarshaler.ParseArray(d, "PlayerProductPurchaseInfos", JM_NShared_PlayerProductPurchaseInfo.Parse);
		o.TopClearDungeonInfos = BaseJsonMarshaler.ParseArray(d, "TopClearDungeonInfos", JM_NShared_TopClearDungeonInfo.Parse);
		o.Result = BaseJsonMarshaler.ParseEnum<ResultType>(d, "Result");
		o.BaseResult = BaseJsonMarshaler.ParseEnum<BaseResultType>(d, "BaseResult");
		o.InternalErrorMessage = BaseJsonMarshaler.ParseValueString(d, "InternalErrorMessage");
		o.AchievementInfos = BaseJsonMarshaler.ParseArray(d, "AchievementInfos", JM_NShared_AchievementInfo.Parse);
		o.ReservedScenarioQuestInfo = BaseJsonMarshaler.ParseObject(d, "ReservedScenarioQuestInfo", JM_NShared_ScenarioQuestInfo.Parse);
		o.ReservedStaminaResults = BaseJsonMarshaler.ParseArray(d, "ReservedStaminaResults", JM_NShared_StaminaResultInfo.Parse);
		o.ReservedPlayRecordInfos = BaseJsonMarshaler.ParseArray(d, "ReservedPlayRecordInfos", JM_NShared_PlayRecordInfo.Parse);
		o.ReservedPlayerArchiveInfos = BaseJsonMarshaler.ParseArray(d, "ReservedPlayerArchiveInfos", JM_NShared_PlayerArchiveInfo.Parse);
		o.ReservedMainQuestInfo = BaseJsonMarshaler.ParseObject(d, "ReservedMainQuestInfo", JM_NShared_MainQuestInfo.Parse);
		o.ReservedSubQuestInfos = BaseJsonMarshaler.ParseArray(d, "ReservedSubQuestInfos", JM_NShared_SubQuestInfo.Parse);
		o.ReservedClearMissionInfos = BaseJsonMarshaler.ParseArray(d, "ReservedClearMissionInfos", JM_NShared_ClearMissionInfo.Parse);
		o.ReservedUpdatedEquipItemInfos = BaseJsonMarshaler.ParseArray(d, "ReservedUpdatedEquipItemInfos", JM_NShared_UpdatedEquipItemInfo.Parse);
		o.ReservedPurchaseMarketingInfos = BaseJsonMarshaler.ParseArray(d, "ReservedPurchaseMarketingInfos", JM_NShared_PurchaseMarketingInfo.Parse);
		o.ReservePlayerKingsPassExpInfo = BaseJsonMarshaler.ParseObject(d, "ReservePlayerKingsPassExpInfo", JM_NShared_PlayerKingsPassExp.Parse);
		o.ReservedKingsPassMissionInfos = BaseJsonMarshaler.ParseArray(d, "ReservedKingsPassMissionInfos", JM_NShared_KingsPassMission.Parse);
		o.ReservedCurrencyResults = BaseJsonMarshaler.ParseArray(d, "ReservedCurrencyResults", JM_NShared_CurrencyResultInfo3.Parse);
		o.ReservedNoticeInfos = BaseJsonMarshaler.ParseArray(d, "ReservedNoticeInfos", JM_NShared_Notice.Parse);
		o.ReserveTeamLevelBuffInfos = BaseJsonMarshaler.ParseArray(d, "ReserveTeamLevelBuffInfos", JM_NShared_TeamLevelBuff.Parse);
		return o;
	}
}
