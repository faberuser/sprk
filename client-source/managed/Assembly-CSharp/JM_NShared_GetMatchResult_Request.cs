using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.GetMatchResult;

public static class JM_NShared_GetMatchResult_Request
{
	public static NShared.GetMatchResult.Request Parse(string s)
	{
		return Parse(s, new NShared.GetMatchResult.Request());
	}

	public static NShared.GetMatchResult.Request Parse(string s, NShared.GetMatchResult.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.GetMatchResult.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.GetMatchResult.Request());
	}

	public static NShared.GetMatchResult.Request Parse(IDictionary d, NShared.GetMatchResult.Request o)
	{
		o.MatchUid = BaseJsonMarshaler.ParseValueLong(d, "MatchUid");
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.GetMatchResult.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.GetMatchResult.Request o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("MatchUid", o.MatchUid.ToString());
		if (o.SessionId != null)
		{
			dictionary.Add("SessionId", o.SessionId);
		}
		return dictionary;
	}
}
