using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.BeginEclipse;

public static class JM_NShared_BeginEclipse_Request
{
	public static NShared.BeginEclipse.Request Parse(string s)
	{
		return Parse(s, new NShared.BeginEclipse.Request());
	}

	public static NShared.BeginEclipse.Request Parse(string s, NShared.BeginEclipse.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.BeginEclipse.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.BeginEclipse.Request());
	}

	public static NShared.BeginEclipse.Request Parse(IDictionary d, NShared.BeginEclipse.Request o)
	{
		o.MatchIndex = BaseJsonMarshaler.ParseValueInt(d, "MatchIndex");
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.BeginEclipse.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.BeginEclipse.Request o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("MatchIndex", o.MatchIndex.ToString());
		if (o.SessionId != null)
		{
			dictionary.Add("SessionId", o.SessionId);
		}
		return dictionary;
	}
}
