using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.GetWorldBossDailyScoreInfo;

public static class JM_NShared_GetWorldBossDailyScoreInfo_Request
{
	public static NShared.GetWorldBossDailyScoreInfo.Request Parse(string s)
	{
		return Parse(s, new NShared.GetWorldBossDailyScoreInfo.Request());
	}

	public static NShared.GetWorldBossDailyScoreInfo.Request Parse(string s, NShared.GetWorldBossDailyScoreInfo.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.GetWorldBossDailyScoreInfo.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.GetWorldBossDailyScoreInfo.Request());
	}

	public static NShared.GetWorldBossDailyScoreInfo.Request Parse(IDictionary d, NShared.GetWorldBossDailyScoreInfo.Request o)
	{
		o.WorldBossIndex = BaseJsonMarshaler.ParseValueInt(d, "WorldBossIndex");
		o.Season = BaseJsonMarshaler.ParseValueInt(d, "Season");
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.GetWorldBossDailyScoreInfo.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.GetWorldBossDailyScoreInfo.Request o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("WorldBossIndex", o.WorldBossIndex.ToString());
		dictionary.Add("Season", o.Season.ToString());
		if (o.SessionId != null)
		{
			dictionary.Add("SessionId", o.SessionId);
		}
		return dictionary;
	}
}
