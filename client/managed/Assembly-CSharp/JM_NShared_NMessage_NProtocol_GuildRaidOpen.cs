using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.NMessage.NProtocol;

public static class JM_NShared_NMessage_NProtocol_GuildRaidOpen
{
	public static NShared.NMessage.NProtocol.GuildRaidOpen Parse(string s)
	{
		return Parse(s, new NShared.NMessage.NProtocol.GuildRaidOpen());
	}

	public static NShared.NMessage.NProtocol.GuildRaidOpen Parse(string s, NShared.NMessage.NProtocol.GuildRaidOpen o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.NMessage.NProtocol.GuildRaidOpen Parse(IDictionary d)
	{
		return Parse(d, new NShared.NMessage.NProtocol.GuildRaidOpen());
	}

	public static NShared.NMessage.NProtocol.GuildRaidOpen Parse(IDictionary d, NShared.NMessage.NProtocol.GuildRaidOpen o)
	{
		o.Nick = BaseJsonMarshaler.ParseValueString(d, "Nick");
		o.OpenedGuildRaidInfo = BaseJsonMarshaler.ParseObject(d, "OpenedGuildRaidInfo", JM_NShared_GuildRaidDungeonInfo.Parse);
		return o;
	}

	public static string EncodeJsonString(NShared.NMessage.NProtocol.GuildRaidOpen o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.NMessage.NProtocol.GuildRaidOpen o)
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
		dictionary.Add("OpenedGuildRaidInfo", JM_NShared_GuildRaidDungeonInfo.EncodeJsonObject(o.OpenedGuildRaidInfo));
		return dictionary;
	}
}
