using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.QuerySession;

public static class JM_NShared_QuerySession_Request
{
	public static NShared.QuerySession.Request Parse(string s)
	{
		return Parse(s, new NShared.QuerySession.Request());
	}

	public static NShared.QuerySession.Request Parse(string s, NShared.QuerySession.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.QuerySession.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.QuerySession.Request());
	}

	public static NShared.QuerySession.Request Parse(IDictionary d, NShared.QuerySession.Request o)
	{
		o.AccountId = BaseJsonMarshaler.ParseValueLong(d, "AccountId");
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.QuerySession.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.QuerySession.Request o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("AccountId", o.AccountId.ToString());
		if (o.SessionId != null)
		{
			dictionary.Add("SessionId", o.SessionId);
		}
		return dictionary;
	}
}
