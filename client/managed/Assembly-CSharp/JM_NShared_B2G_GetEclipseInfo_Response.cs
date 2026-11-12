using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.B2G_GetEclipseInfo;

public static class JM_NShared_B2G_GetEclipseInfo_Response
{
	public static NShared.B2G_GetEclipseInfo.Response Parse(string s)
	{
		return Parse(s, new NShared.B2G_GetEclipseInfo.Response());
	}

	public static NShared.B2G_GetEclipseInfo.Response Parse(string s, NShared.B2G_GetEclipseInfo.Response o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.B2G_GetEclipseInfo.Response Parse(IDictionary d)
	{
		return Parse(d, new NShared.B2G_GetEclipseInfo.Response());
	}

	public static NShared.B2G_GetEclipseInfo.Response Parse(IDictionary d, NShared.B2G_GetEclipseInfo.Response o)
	{
		o.MatchIndex = BaseJsonMarshaler.ParseValueInt(d, "MatchIndex");
		o.DeckList = BaseJsonMarshaler.ParseArray(d, "DeckList", JM_NShared_EclipseDeckData.Parse);
		o.DungeonList = BaseJsonMarshaler.ParseArray(d, "DungeonList", JM_NShared_EclipseDungeonData.Parse);
		o.AdminLevel = BaseJsonMarshaler.ParseValueInt(d, "AdminLevel");
		o.GuildSkills = BaseJsonMarshaler.ParseArray(d, "GuildSkills", JM_NShared_GuildSkillDataBase.Parse);
		o.AccountBuffs = BaseJsonMarshaler.ParseArray(d, "AccountBuffs", JM_NShared_AccountBuffDataBase.Parse);
		o.ClassBuffDataBases = BaseJsonMarshaler.ParseArray(d, "ClassBuffDataBases", JM_NShared_ClassBuffDataBase.Parse);
		o.CurrentDeckIndex = BaseJsonMarshaler.ParseValueInt(d, "CurrentDeckIndex");
		o.CurrentChapterIndex = BaseJsonMarshaler.ParseValueInt(d, "CurrentChapterIndex");
		o.CurrentDungeonIndex = BaseJsonMarshaler.ParseValueInt(d, "CurrentDungeonIndex");
		o.CurrentWaveIndex = BaseJsonMarshaler.ParseValueInt(d, "CurrentWaveIndex");
		o.MaxWaveIndex = BaseJsonMarshaler.ParseValueInt(d, "MaxWaveIndex");
		o.CurrentCreatureLevel = BaseJsonMarshaler.ParseValueInt(d, "CurrentCreatureLevel");
		o.GameServerSessionKey = BaseJsonMarshaler.ParseValueString(d, "GameServerSessionKey");
		o.OnlineGameSpeedRatio = BaseJsonMarshaler.ParseValueInt(d, "OnlineGameSpeedRatio");
		o.ExtraStatDataBases = BaseJsonMarshaler.ParseArray(d, "ExtraStatDataBases", JM_NShared_DefaultExtraStatData.Parse);
		o.PetStatDataBases = BaseJsonMarshaler.ParseArray(d, "PetStatDataBases", JM_NShared_PetStatDataBase.Parse);
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

	public static string EncodeJsonString(NShared.B2G_GetEclipseInfo.Response o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.B2G_GetEclipseInfo.Response o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("MatchIndex", o.MatchIndex.ToString());
		dictionary.Add("DeckList", BaseJsonMarshaler.EncodeArray(o.DeckList, JM_NShared_EclipseDeckData.EncodeJsonObject));
		dictionary.Add("DungeonList", BaseJsonMarshaler.EncodeArray(o.DungeonList, JM_NShared_EclipseDungeonData.EncodeJsonObject));
		dictionary.Add("AdminLevel", o.AdminLevel.ToString());
		dictionary.Add("GuildSkills", BaseJsonMarshaler.EncodeArray(o.GuildSkills, JM_NShared_GuildSkillDataBase.EncodeJsonObject));
		dictionary.Add("AccountBuffs", BaseJsonMarshaler.EncodeArray(o.AccountBuffs, JM_NShared_AccountBuffDataBase.EncodeJsonObject));
		dictionary.Add("ClassBuffDataBases", BaseJsonMarshaler.EncodeArray(o.ClassBuffDataBases, JM_NShared_ClassBuffDataBase.EncodeJsonObject));
		dictionary.Add("CurrentDeckIndex", o.CurrentDeckIndex.ToString());
		dictionary.Add("CurrentChapterIndex", o.CurrentChapterIndex.ToString());
		dictionary.Add("CurrentDungeonIndex", o.CurrentDungeonIndex.ToString());
		dictionary.Add("CurrentWaveIndex", o.CurrentWaveIndex.ToString());
		dictionary.Add("MaxWaveIndex", o.MaxWaveIndex.ToString());
		dictionary.Add("CurrentCreatureLevel", o.CurrentCreatureLevel.ToString());
		if (o.GameServerSessionKey != null)
		{
			dictionary.Add("GameServerSessionKey", o.GameServerSessionKey);
		}
		dictionary.Add("OnlineGameSpeedRatio", o.OnlineGameSpeedRatio.ToString());
		dictionary.Add("ExtraStatDataBases", BaseJsonMarshaler.EncodeArray(o.ExtraStatDataBases, JM_NShared_DefaultExtraStatData.EncodeJsonObject));
		dictionary.Add("PetStatDataBases", BaseJsonMarshaler.EncodeArray(o.PetStatDataBases, JM_NShared_PetStatDataBase.EncodeJsonObject));
		dictionary.Add("Result", o.Result.ToString());
		dictionary.Add("BaseResult", o.BaseResult.ToString());
		if (o.InternalErrorMessage != null)
		{
			dictionary.Add("InternalErrorMessage", o.InternalErrorMessage);
		}
		dictionary.Add("AchievementInfos", BaseJsonMarshaler.EncodeArray(o.AchievementInfos, JM_NShared_AchievementInfo.EncodeJsonObject));
		dictionary.Add("ReservedScenarioQuestInfo", JM_NShared_ScenarioQuestInfo.EncodeJsonObject(o.ReservedScenarioQuestInfo));
		dictionary.Add("ReservedStaminaResults", BaseJsonMarshaler.EncodeArray(o.ReservedStaminaResults, JM_NShared_StaminaResultInfo.EncodeJsonObject));
		dictionary.Add("ReservedPlayRecordInfos", BaseJsonMarshaler.EncodeArray(o.ReservedPlayRecordInfos, JM_NShared_PlayRecordInfo.EncodeJsonObject));
		dictionary.Add("ReservedPlayerArchiveInfos", BaseJsonMarshaler.EncodeArray(o.ReservedPlayerArchiveInfos, JM_NShared_PlayerArchiveInfo.EncodeJsonObject));
		dictionary.Add("ReservedMainQuestInfo", JM_NShared_MainQuestInfo.EncodeJsonObject(o.ReservedMainQuestInfo));
		dictionary.Add("ReservedSubQuestInfos", BaseJsonMarshaler.EncodeArray(o.ReservedSubQuestInfos, JM_NShared_SubQuestInfo.EncodeJsonObject));
		dictionary.Add("ReservedClearMissionInfos", BaseJsonMarshaler.EncodeArray(o.ReservedClearMissionInfos, JM_NShared_ClearMissionInfo.EncodeJsonObject));
		dictionary.Add("ReservedUpdatedEquipItemInfos", BaseJsonMarshaler.EncodeArray(o.ReservedUpdatedEquipItemInfos, JM_NShared_UpdatedEquipItemInfo.EncodeJsonObject));
		dictionary.Add("ReservedPurchaseMarketingInfos", BaseJsonMarshaler.EncodeArray(o.ReservedPurchaseMarketingInfos, JM_NShared_PurchaseMarketingInfo.EncodeJsonObject));
		dictionary.Add("ReservePlayerKingsPassExpInfo", JM_NShared_PlayerKingsPassExp.EncodeJsonObject(o.ReservePlayerKingsPassExpInfo));
		dictionary.Add("ReservedKingsPassMissionInfos", BaseJsonMarshaler.EncodeArray(o.ReservedKingsPassMissionInfos, JM_NShared_KingsPassMission.EncodeJsonObject));
		dictionary.Add("ReservedCurrencyResults", BaseJsonMarshaler.EncodeArray(o.ReservedCurrencyResults, JM_NShared_CurrencyResultInfo3.EncodeJsonObject));
		dictionary.Add("ReservedNoticeInfos", BaseJsonMarshaler.EncodeArray(o.ReservedNoticeInfos, JM_NShared_Notice.EncodeJsonObject));
		dictionary.Add("ReserveTeamLevelBuffInfos", BaseJsonMarshaler.EncodeArray(o.ReserveTeamLevelBuffInfos, JM_NShared_TeamLevelBuff.EncodeJsonObject));
		return dictionary;
	}
}
