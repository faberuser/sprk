using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_GuildArenaBuffDataBase
{
	public static GuildArenaBuffDataBase Parse(string s)
	{
		return Parse(s, new GuildArenaBuffDataBase());
	}

	public static GuildArenaBuffDataBase Parse(string s, GuildArenaBuffDataBase o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static GuildArenaBuffDataBase Parse(IDictionary d)
	{
		return Parse(d, new GuildArenaBuffDataBase());
	}

	public static GuildArenaBuffDataBase Parse(IDictionary d, GuildArenaBuffDataBase o)
	{
		o.SkillIndex = BaseJsonMarshaler.ParseValueInt(d, "SkillIndex");
		return o;
	}

	public static string EncodeJsonString(GuildArenaBuffDataBase o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(GuildArenaBuffDataBase o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("SkillIndex", o.SkillIndex.ToString());
		return dictionary;
	}
}
