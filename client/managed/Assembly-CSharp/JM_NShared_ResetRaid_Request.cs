using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.ResetRaid;

public static class JM_NShared_ResetRaid_Request
{
	public static NShared.ResetRaid.Request Parse(string s)
	{
		return Parse(s, new NShared.ResetRaid.Request());
	}

	public static NShared.ResetRaid.Request Parse(string s, NShared.ResetRaid.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.ResetRaid.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.ResetRaid.Request());
	}

	public static NShared.ResetRaid.Request Parse(IDictionary d, NShared.ResetRaid.Request o)
	{
		o.RaidIndex = BaseJsonMarshaler.ParseValueInt(d, "RaidIndex");
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.ResetRaid.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.ResetRaid.Request o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("RaidIndex", o.RaidIndex.ToString());
		if (o.SessionId != null)
		{
			dictionary.Add("SessionId", o.SessionId);
		}
		return dictionary;
	}
}
