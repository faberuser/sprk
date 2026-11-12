using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.NMessage.NProtocol;

public static class JM_NShared_NMessage_NProtocol_GuildSuppressKilled
{
	public static GuildSuppressKilled Parse(string s)
	{
		return Parse(s, new GuildSuppressKilled());
	}

	public static GuildSuppressKilled Parse(string s, GuildSuppressKilled o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static GuildSuppressKilled Parse(IDictionary d)
	{
		return Parse(d, new GuildSuppressKilled());
	}

	public static GuildSuppressKilled Parse(IDictionary d, GuildSuppressKilled o)
	{
		o.KillAccountIds = BaseJsonMarshaler.ParseArrayCast(d, "KillAccountIds", BaseJsonMarshaler.CastToLong);
		o.Level = BaseJsonMarshaler.ParseValueInt(d, "Level");
		return o;
	}

	public static string EncodeJsonString(GuildSuppressKilled o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(GuildSuppressKilled o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("KillAccountIds", BaseJsonMarshaler.EncodeArray(o.KillAccountIds));
		dictionary.Add("Level", o.Level.ToString());
		return dictionary;
	}
}
