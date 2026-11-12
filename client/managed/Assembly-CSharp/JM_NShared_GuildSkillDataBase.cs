using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_GuildSkillDataBase
{
	public static GuildSkillDataBase Parse(string s)
	{
		return Parse(s, new GuildSkillDataBase());
	}

	public static GuildSkillDataBase Parse(string s, GuildSkillDataBase o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static GuildSkillDataBase Parse(IDictionary d)
	{
		return Parse(d, new GuildSkillDataBase());
	}

	public static GuildSkillDataBase Parse(IDictionary d, GuildSkillDataBase o)
	{
		o.GuildSkillIndex = BaseJsonMarshaler.ParseValueInt(d, "GuildSkillIndex");
		o.SkillIndex = BaseJsonMarshaler.ParseValueInt(d, "SkillIndex");
		o.GuildSkillLevel = BaseJsonMarshaler.ParseValueByte(d, "GuildSkillLevel");
		return o;
	}

	public static string EncodeJsonString(GuildSkillDataBase o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(GuildSkillDataBase o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("GuildSkillIndex", o.GuildSkillIndex.ToString());
		dictionary.Add("SkillIndex", o.SkillIndex.ToString());
		dictionary.Add("GuildSkillLevel", o.GuildSkillLevel.ToString());
		return dictionary;
	}
}
