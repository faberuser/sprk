using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.GetGuildArenaRanker;

public static class JM_NShared_GetGuildArenaRanker_Request
{
	public static NShared.GetGuildArenaRanker.Request Parse(string s)
	{
		return Parse(s, new NShared.GetGuildArenaRanker.Request());
	}

	public static NShared.GetGuildArenaRanker.Request Parse(string s, NShared.GetGuildArenaRanker.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.GetGuildArenaRanker.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.GetGuildArenaRanker.Request());
	}

	public static NShared.GetGuildArenaRanker.Request Parse(IDictionary d, NShared.GetGuildArenaRanker.Request o)
	{
		o.SeasonIndex = BaseJsonMarshaler.ParseValueInt(d, "SeasonIndex");
		o.PageNo = BaseJsonMarshaler.ParseValueInt(d, "PageNo");
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.GetGuildArenaRanker.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.GetGuildArenaRanker.Request o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("SeasonIndex", o.SeasonIndex.ToString());
		dictionary.Add("PageNo", o.PageNo.ToString());
		if (o.SessionId != null)
		{
			dictionary.Add("SessionId", o.SessionId);
		}
		return dictionary;
	}
}
