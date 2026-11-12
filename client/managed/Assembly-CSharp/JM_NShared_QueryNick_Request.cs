using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.QueryNick;

public static class JM_NShared_QueryNick_Request
{
	public static NShared.QueryNick.Request Parse(string s)
	{
		return Parse(s, new NShared.QueryNick.Request());
	}

	public static NShared.QueryNick.Request Parse(string s, NShared.QueryNick.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.QueryNick.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.QueryNick.Request());
	}

	public static NShared.QueryNick.Request Parse(IDictionary d, NShared.QueryNick.Request o)
	{
		o.Nick = BaseJsonMarshaler.ParseValueString(d, "Nick");
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.QueryNick.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.QueryNick.Request o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		if (o.Nick != null)
		{
			dictionary.Add("Nick", o.Nick);
		}
		if (o.SessionId != null)
		{
			dictionary.Add("SessionId", o.SessionId);
		}
		return dictionary;
	}
}
