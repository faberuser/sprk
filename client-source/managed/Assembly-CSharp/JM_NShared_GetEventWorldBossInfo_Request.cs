using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.GetEventWorldBossInfo;

public static class JM_NShared_GetEventWorldBossInfo_Request
{
	public static NShared.GetEventWorldBossInfo.Request Parse(string s)
	{
		return Parse(s, new NShared.GetEventWorldBossInfo.Request());
	}

	public static NShared.GetEventWorldBossInfo.Request Parse(string s, NShared.GetEventWorldBossInfo.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.GetEventWorldBossInfo.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.GetEventWorldBossInfo.Request());
	}

	public static NShared.GetEventWorldBossInfo.Request Parse(IDictionary d, NShared.GetEventWorldBossInfo.Request o)
	{
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.GetEventWorldBossInfo.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.GetEventWorldBossInfo.Request o)
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
