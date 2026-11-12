using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.NMessage.NProtocol;

public static class JM_NShared_NMessage_NProtocol_GuildRaidEnd
{
	public static GuildRaidEnd Parse(string s)
	{
		return Parse(s, new GuildRaidEnd());
	}

	public static GuildRaidEnd Parse(string s, GuildRaidEnd o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static GuildRaidEnd Parse(IDictionary d)
	{
		return Parse(d, new GuildRaidEnd());
	}

	public static GuildRaidEnd Parse(IDictionary d, GuildRaidEnd o)
	{
		o.Nick = BaseJsonMarshaler.ParseValueString(d, "Nick");
		o.ChapterIndex = BaseJsonMarshaler.ParseValueInt(d, "ChapterIndex");
		o.DungeonIndex = BaseJsonMarshaler.ParseValueInt(d, "DungeonIndex");
		o.ResultGuildRaidInfo = BaseJsonMarshaler.ParseObject(d, "ResultGuildRaidInfo", JM_NShared_GuildRaidDungeonInfo.Parse);
		o.Completed = BaseJsonMarshaler.ParseValueBool(d, "Completed");
		return o;
	}

	public static string EncodeJsonString(GuildRaidEnd o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(GuildRaidEnd o)
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
		dictionary.Add("ResultGuildRaidInfo", JM_NShared_GuildRaidDungeonInfo.EncodeJsonObject(o.ResultGuildRaidInfo));
		dictionary.Add("Completed", o.Completed.ToString());
		return dictionary;
	}
}
