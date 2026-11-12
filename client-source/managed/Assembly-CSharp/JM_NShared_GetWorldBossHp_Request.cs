using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.GetWorldBossHp;

public static class JM_NShared_GetWorldBossHp_Request
{
	public static NShared.GetWorldBossHp.Request Parse(string s)
	{
		return Parse(s, new NShared.GetWorldBossHp.Request());
	}

	public static NShared.GetWorldBossHp.Request Parse(string s, NShared.GetWorldBossHp.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.GetWorldBossHp.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.GetWorldBossHp.Request());
	}

	public static NShared.GetWorldBossHp.Request Parse(IDictionary d, NShared.GetWorldBossHp.Request o)
	{
		o.WorldBossIndex = BaseJsonMarshaler.ParseValueInt(d, "WorldBossIndex");
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.GetWorldBossHp.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.GetWorldBossHp.Request o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("WorldBossIndex", o.WorldBossIndex.ToString());
		if (o.SessionId != null)
		{
			dictionary.Add("SessionId", o.SessionId);
		}
		return dictionary;
	}
}
