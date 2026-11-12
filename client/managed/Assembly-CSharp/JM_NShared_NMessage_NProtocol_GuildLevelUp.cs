using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.NMessage.NProtocol;

public static class JM_NShared_NMessage_NProtocol_GuildLevelUp
{
	public static GuildLevelUp Parse(string s)
	{
		return Parse(s, new GuildLevelUp());
	}

	public static GuildLevelUp Parse(string s, GuildLevelUp o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static GuildLevelUp Parse(IDictionary d)
	{
		return Parse(d, new GuildLevelUp());
	}

	public static GuildLevelUp Parse(IDictionary d, GuildLevelUp o)
	{
		o.Level = BaseJsonMarshaler.ParseValueByte(d, "Level");
		return o;
	}

	public static string EncodeJsonString(GuildLevelUp o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(GuildLevelUp o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("Level", o.Level.ToString());
		return dictionary;
	}
}
