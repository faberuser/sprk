using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.EndEclipse;

public static class JM_NShared_EndEclipse_Request
{
	public static NShared.EndEclipse.Request Parse(string s)
	{
		return Parse(s, new NShared.EndEclipse.Request());
	}

	public static NShared.EndEclipse.Request Parse(string s, NShared.EndEclipse.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.EndEclipse.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.EndEclipse.Request());
	}

	public static NShared.EndEclipse.Request Parse(IDictionary d, NShared.EndEclipse.Request o)
	{
		o.MatchIndex = BaseJsonMarshaler.ParseValueInt(d, "MatchIndex");
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.EndEclipse.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.EndEclipse.Request o)
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
