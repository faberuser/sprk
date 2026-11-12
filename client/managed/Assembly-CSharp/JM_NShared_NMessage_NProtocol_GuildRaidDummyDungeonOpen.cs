using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.NMessage.NProtocol;

public static class JM_NShared_NMessage_NProtocol_GuildRaidDummyDungeonOpen
{
	public static GuildRaidDummyDungeonOpen Parse(string s)
	{
		return Parse(s, new GuildRaidDummyDungeonOpen());
	}

	public static GuildRaidDummyDungeonOpen Parse(string s, GuildRaidDummyDungeonOpen o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static GuildRaidDummyDungeonOpen Parse(IDictionary d)
	{
		return Parse(d, new GuildRaidDummyDungeonOpen());
	}

	public static GuildRaidDummyDungeonOpen Parse(IDictionary d, GuildRaidDummyDungeonOpen o)
	{
		o.Nick = BaseJsonMarshaler.ParseValueString(d, "Nick");
		return o;
	}

	public static string EncodeJsonString(GuildRaidDummyDungeonOpen o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(GuildRaidDummyDungeonOpen o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		if (o.Nick != null)
		{
			dictionary.Add("Nick", o.Nick);
		}
		return dictionary;
	}
}
