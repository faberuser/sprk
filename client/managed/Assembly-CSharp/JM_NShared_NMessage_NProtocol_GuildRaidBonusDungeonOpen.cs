using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.NMessage.NProtocol;

public static class JM_NShared_NMessage_NProtocol_GuildRaidBonusDungeonOpen
{
	public static GuildRaidBonusDungeonOpen Parse(string s)
	{
		return Parse(s, new GuildRaidBonusDungeonOpen());
	}

	public static GuildRaidBonusDungeonOpen Parse(string s, GuildRaidBonusDungeonOpen o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static GuildRaidBonusDungeonOpen Parse(IDictionary d)
	{
		return Parse(d, new GuildRaidBonusDungeonOpen());
	}

	public static GuildRaidBonusDungeonOpen Parse(IDictionary d, GuildRaidBonusDungeonOpen o)
	{
		o.Nick = BaseJsonMarshaler.ParseValueString(d, "Nick");
		return o;
	}

	public static string EncodeJsonString(GuildRaidBonusDungeonOpen o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(GuildRaidBonusDungeonOpen o)
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
