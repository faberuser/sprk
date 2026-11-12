using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.NMessage.NProtocol;

public static class JM_NShared_NMessage_NProtocol_WorldBossKilled
{
	public static WorldBossKilled Parse(string s)
	{
		return Parse(s, new WorldBossKilled());
	}

	public static WorldBossKilled Parse(string s, WorldBossKilled o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static WorldBossKilled Parse(IDictionary d)
	{
		return Parse(d, new WorldBossKilled());
	}

	public static WorldBossKilled Parse(IDictionary d, WorldBossKilled o)
	{
		o.WorldBossIndex = BaseJsonMarshaler.ParseValueInt(d, "WorldBossIndex");
		o.KillAccountId = BaseJsonMarshaler.ParseValueLong(d, "KillAccountId");
		o.KillUserName = BaseJsonMarshaler.ParseValueString(d, "KillUserName");
		return o;
	}

	public static string EncodeJsonString(WorldBossKilled o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(WorldBossKilled o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("WorldBossIndex", o.WorldBossIndex.ToString());
		dictionary.Add("KillAccountId", o.KillAccountId.ToString());
		if (o.KillUserName != null)
		{
			dictionary.Add("KillUserName", o.KillUserName);
		}
		return dictionary;
	}
}
