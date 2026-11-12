using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.NMessage.NProtocol;

public static class JM_NShared_NMessage_NProtocol_GuildRaidBegin
{
	public static GuildRaidBegin Parse(string s)
	{
		return Parse(s, new GuildRaidBegin());
	}

	public static GuildRaidBegin Parse(string s, GuildRaidBegin o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static GuildRaidBegin Parse(IDictionary d)
	{
		return Parse(d, new GuildRaidBegin());
	}

	public static GuildRaidBegin Parse(IDictionary d, GuildRaidBegin o)
	{
		o.Nick = BaseJsonMarshaler.ParseValueString(d, "Nick");
		o.ChapterIndex = BaseJsonMarshaler.ParseValueInt(d, "ChapterIndex");
		o.DungeonIndex = BaseJsonMarshaler.ParseValueInt(d, "DungeonIndex");
		return o;
	}

	public static string EncodeJsonString(GuildRaidBegin o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(GuildRaidBegin o)
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
		dictionary.Add("ChapterIndex", o.ChapterIndex.ToString());
		dictionary.Add("DungeonIndex", o.DungeonIndex.ToString());
		return dictionary;
	}
}
