using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.GetGuildArenaMemberScore;

public static class JM_NShared_GetGuildArenaMemberScore_Request
{
	public static NShared.GetGuildArenaMemberScore.Request Parse(string s)
	{
		return Parse(s, new NShared.GetGuildArenaMemberScore.Request());
	}

	public static NShared.GetGuildArenaMemberScore.Request Parse(string s, NShared.GetGuildArenaMemberScore.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.GetGuildArenaMemberScore.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.GetGuildArenaMemberScore.Request());
	}

	public static NShared.GetGuildArenaMemberScore.Request Parse(IDictionary d, NShared.GetGuildArenaMemberScore.Request o)
	{
		o.SessionIndex = BaseJsonMarshaler.ParseValueInt(d, "SessionIndex");
		o.PageNo = BaseJsonMarshaler.ParseValueInt(d, "PageNo");
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.GetGuildArenaMemberScore.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.GetGuildArenaMemberScore.Request o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("SessionIndex", o.SessionIndex.ToString());
		dictionary.Add("PageNo", o.PageNo.ToString());
		if (o.SessionId != null)
		{
			dictionary.Add("SessionId", o.SessionId);
		}
		return dictionary;
	}
}
