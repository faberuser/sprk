using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.Ping;

public static class JM_NShared_Ping_Request
{
	public static NShared.Ping.Request Parse(string s)
	{
		return Parse(s, new NShared.Ping.Request());
	}

	public static NShared.Ping.Request Parse(string s, NShared.Ping.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.Ping.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.Ping.Request());
	}

	public static NShared.Ping.Request Parse(IDictionary d, NShared.Ping.Request o)
	{
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.Ping.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.Ping.Request o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		if (o.SessionId != null)
		{
			dictionary.Add("SessionId", o.SessionId);
		}
		return dictionary;
	}
}
