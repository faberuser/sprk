using System.Collections;
using MiniJSON;
using NShared;
using NShared.WaitMatch;

public static class JM_NShared_WaitMatch_Response
{
	public static NShared.WaitMatch.Response Parse(string s)
	{
		return Parse(s, new NShared.WaitMatch.Response());
	}

	public static NShared.WaitMatch.Response Parse(string s, NShared.WaitMatch.Response o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.WaitMatch.Response Parse(IDictionary d)
	{
		return Parse(d, new NShared.WaitMatch.Response());
	}

	public static NShared.WaitMatch.Response Parse(IDictionary d, NShared.WaitMatch.Response o)
	{
		o.MatchInfo = BaseJsonMarshaler.ParseObject(d, "MatchInfo", JM_NShared_MatchInfo.Parse);
		o.MatchedNpcInfo = BaseJsonMarshaler.ParseObject(d, "MatchedNpcInfo", JM_NShared_MatchedAccountInfo.Parse);
		o.SwordResult = BaseJsonMarshaler.ParseObject(d, "SwordResult", JM_NShared_StaminaResultInfo.Parse);
		o.ChapterIndex = BaseJsonMarshaler.ParseValueInt(d, "ChapterIndex");
		o.DungeonIndex = BaseJsonMarshaler.ParseValueInt(d, "DungeonIndex");
		o.BattleInfo = BaseJsonMarshaler.ParseObject(d, "BattleInfo", JM_NShared_PlayerBattleInfo.Parse);
		o.RankResult = BaseJsonMarshaler.ParseObject(d, "RankResult", JM_NShared_RankResultInfo.Parse);
		o.RelayBattleServerAddress = BaseJsonMarshaler.ParseValueString(d, "RelayBattleServerAddress");
		o.RelayBattleServerPort = BaseJsonMarshaler.ParseValueInt(d, "RelayBattleServerPort");
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
