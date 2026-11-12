using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.ResetAllRaid;

public static class JM_NShared_ResetAllRaid_Request
{
	public static NShared.ResetAllRaid.Request Parse(string s)
	{
		return Parse(s, new NShared.ResetAllRaid.Request());
	}

	public static NShared.ResetAllRaid.Request Parse(string s, NShared.ResetAllRaid.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.ResetAllRaid.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.ResetAllRaid.Request());
	}

	public static NShared.ResetAllRaid.Request Parse(IDictionary d, NShared.ResetAllRaid.Request o)
	{
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.ResetAllRaid.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.ResetAllRaid.Request o)
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
