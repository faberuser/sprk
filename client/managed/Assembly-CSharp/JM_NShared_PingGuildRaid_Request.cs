using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.PingGuildRaid;

public static class JM_NShared_PingGuildRaid_Request
{
	public static NShared.PingGuildRaid.Request Parse(string s)
	{
		return Parse(s, new NShared.PingGuildRaid.Request());
	}

	public static NShared.PingGuildRaid.Request Parse(string s, NShared.PingGuildRaid.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.PingGuildRaid.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.PingGuildRaid.Request());
	}

	public static NShared.PingGuildRaid.Request Parse(IDictionary d, NShared.PingGuildRaid.Request o)
	{
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.PingGuildRaid.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.PingGuildRaid.Request o)
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
