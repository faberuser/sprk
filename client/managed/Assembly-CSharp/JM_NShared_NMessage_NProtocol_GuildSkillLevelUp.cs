using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.NMessage.NProtocol;

public static class JM_NShared_NMessage_NProtocol_GuildSkillLevelUp
{
	public static GuildSkillLevelUp Parse(string s)
	{
		return Parse(s, new GuildSkillLevelUp());
	}

	public static GuildSkillLevelUp Parse(string s, GuildSkillLevelUp o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static GuildSkillLevelUp Parse(IDictionary d)
	{
		return Parse(d, new GuildSkillLevelUp());
	}

	public static GuildSkillLevelUp Parse(IDictionary d, GuildSkillLevelUp o)
	{
		o.GuildSkillIndex = BaseJsonMarshaler.ParseValueInt(d, "GuildSkillIndex");
		o.Level = BaseJsonMarshaler.ParseValueByte(d, "Level");
		return o;
	}

	public static string EncodeJsonString(GuildSkillLevelUp o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(GuildSkillLevelUp o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("GuildSkillIndex", o.GuildSkillIndex.ToString());
		dictionary.Add("Level", o.Level.ToString());
		return dictionary;
	}
}
