using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.GetEventWorldBossScoreInfo;

public static class JM_NShared_GetEventWorldBossScoreInfo_Request
{
	public static NShared.GetEventWorldBossScoreInfo.Request Parse(string s)
	{
		return Parse(s, new NShared.GetEventWorldBossScoreInfo.Request());
	}

	public static NShared.GetEventWorldBossScoreInfo.Request Parse(string s, NShared.GetEventWorldBossScoreInfo.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.GetEventWorldBossScoreInfo.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.GetEventWorldBossScoreInfo.Request());
	}

	public static NShared.GetEventWorldBossScoreInfo.Request Parse(IDictionary d, NShared.GetEventWorldBossScoreInfo.Request o)
	{
		o.EventWorldBossIndex = BaseJsonMarshaler.ParseValueInt(d, "EventWorldBossIndex");
		o.Season = BaseJsonMarshaler.ParseValueInt(d, "Season");
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.GetEventWorldBossScoreInfo.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.GetEventWorldBossScoreInfo.Request o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("EventWorldBossIndex", o.EventWorldBossIndex.ToString());
		dictionary.Add("Season", o.Season.ToString());
		if (o.SessionId != null)
		{
			dictionary.Add("SessionId", o.SessionId);
		}
		return dictionary;
	}
}
