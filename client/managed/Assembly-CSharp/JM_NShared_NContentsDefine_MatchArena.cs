using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.NContentsDefine;

public static class JM_NShared_NContentsDefine_MatchArena
{
	public static MatchArena Parse(string s)
	{
		return Parse(s, new MatchArena());
	}

	public static MatchArena Parse(string s, MatchArena o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static MatchArena Parse(IDictionary d)
	{
		return Parse(d, new MatchArena());
	}

	public static MatchArena Parse(IDictionary d, MatchArena o)
	{
		o.ArenaSeasonDesc = BaseJsonMarshaler.ParseDictionary<MatchArenaType, MatchArena.SeasonDesc, Dictionary<MatchArenaType, MatchArena.SeasonDesc>>(d, "ArenaSeasonDesc", JM_NShared_NContentsDefine_MatchArena_SeasonDesc.Parse);
		o.OrdealArena_GiveRewardTime = BaseJsonMarshaler.ParseObject(d, "OrdealArena_GiveRewardTime", JM_NShared_NContentsDefine_MatchArena_TimeWithDayOfWeek.Parse);
		o.WaitingTime_for_RequestOfflineMatch = BaseJsonMarshaler.ParseValueFloat(d, "WaitingTime_for_RequestOfflineMatch");
		o.IntervalTime_for_RequestWaitMatch = BaseJsonMarshaler.ParseValueFloat(d, "IntervalTime_for_RequestWaitMatch");
		o.DelayTime_for_EnterBattleScene = BaseJsonMarshaler.ParseValueFloat(d, "DelayTime_for_EnterBattleScene");
		o.Duration_Short = BaseJsonMarshaler.ParseValueFloat(d, "Duration_Short");
		o.CurrentSeason_RankerList_Duration_Long = BaseJsonMarshaler.ParseValueFloat(d, "CurrentSeason_RankerList_Duration_Long");
		o.PrevSeason_RankerList_Duration = BaseJsonMarshaler.ParseValueFloat(d, "PrevSeason_RankerList_Duration");
		o.Champions_of_Match = BaseJsonMarshaler.ParseList<MatchArena.ChampionOfMatch, List<MatchArena.ChampionOfMatch>>(d, "Champions_of_Match", JM_NShared_NContentsDefine_MatchArena_ChampionOfMatch.Parse);
		return o;
	}

	public static string EncodeJsonString(MatchArena o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(MatchArena o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("ArenaSeasonDesc", BaseJsonMarshaler.EncodeDictionary(o.ArenaSeasonDesc, JM_NShared_NContentsDefine_MatchArena_SeasonDesc.EncodeJsonObject));
		dictionary.Add("OrdealArena_GiveRewardTime", JM_NShared_NContentsDefine_MatchArena_TimeWithDayOfWeek.EncodeJsonObject(o.OrdealArena_GiveRewardTime));
		dictionary.Add("WaitingTime_for_RequestOfflineMatch", o.WaitingTime_for_RequestOfflineMatch.ToString());
		dictionary.Add("IntervalTime_for_RequestWaitMatch", o.IntervalTime_for_RequestWaitMatch.ToString());
		dictionary.Add("DelayTime_for_EnterBattleScene", o.DelayTime_for_EnterBattleScene.ToString());
		dictionary.Add("Duration_Short", o.Duration_Short.ToString());
		dictionary.Add("CurrentSeason_RankerList_Duration_Long", o.CurrentSeason_RankerList_Duration_Long.ToString());
		dictionary.Add("PrevSeason_RankerList_Duration", o.PrevSeason_RankerList_Duration.ToString());
		dictionary.Add("Champions_of_Match", BaseJsonMarshaler.EncodeArray(o.Champions_of_Match, JM_NShared_NContentsDefine_MatchArena_ChampionOfMatch.EncodeJsonObject));
		return dictionary;
	}
}
