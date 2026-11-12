using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.GetWorldBossInfo;

public static class JM_NShared_GetWorldBossInfo_Request
{
	public static NShared.GetWorldBossInfo.Request Parse(string s)
	{
		return Parse(s, new NShared.GetWorldBossInfo.Request());
	}

	public static NShared.GetWorldBossInfo.Request Parse(string s, NShared.GetWorldBossInfo.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.GetWorldBossInfo.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.GetWorldBossInfo.Request());
	}

	public static NShared.GetWorldBossInfo.Request Parse(IDictionary d, NShared.GetWorldBossInfo.Request o)
	{
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.GetWorldBossInfo.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.GetWorldBossInfo.Request o)
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
