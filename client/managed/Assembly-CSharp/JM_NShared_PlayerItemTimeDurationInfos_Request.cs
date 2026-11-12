using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.PlayerItemTimeDurationInfos;

public static class JM_NShared_PlayerItemTimeDurationInfos_Request
{
	public static NShared.PlayerItemTimeDurationInfos.Request Parse(string s)
	{
		return Parse(s, new NShared.PlayerItemTimeDurationInfos.Request());
	}

	public static NShared.PlayerItemTimeDurationInfos.Request Parse(string s, NShared.PlayerItemTimeDurationInfos.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.PlayerItemTimeDurationInfos.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.PlayerItemTimeDurationInfos.Request());
	}

	public static NShared.PlayerItemTimeDurationInfos.Request Parse(IDictionary d, NShared.PlayerItemTimeDurationInfos.Request o)
	{
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.PlayerItemTimeDurationInfos.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.PlayerItemTimeDurationInfos.Request o)
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
