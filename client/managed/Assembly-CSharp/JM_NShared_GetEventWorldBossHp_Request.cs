using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.GetEventWorldBossHp;

public static class JM_NShared_GetEventWorldBossHp_Request
{
	public static NShared.GetEventWorldBossHp.Request Parse(string s)
	{
		return Parse(s, new NShared.GetEventWorldBossHp.Request());
	}

	public static NShared.GetEventWorldBossHp.Request Parse(string s, NShared.GetEventWorldBossHp.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.GetEventWorldBossHp.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.GetEventWorldBossHp.Request());
	}

	public static NShared.GetEventWorldBossHp.Request Parse(IDictionary d, NShared.GetEventWorldBossHp.Request o)
	{
		o.EventWorldBossIndex = BaseJsonMarshaler.ParseValueInt(d, "EventWorldBossIndex");
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.GetEventWorldBossHp.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.GetEventWorldBossHp.Request o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("EventWorldBossIndex", o.EventWorldBossIndex.ToString());
		if (o.SessionId != null)
		{
			dictionary.Add("SessionId", o.SessionId);
		}
		return dictionary;
	}
}
