using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.LevelUpGuildSkill;

public static class JM_NShared_LevelUpGuildSkill_Request
{
	public static NShared.LevelUpGuildSkill.Request Parse(string s)
	{
		return Parse(s, new NShared.LevelUpGuildSkill.Request());
	}

	public static NShared.LevelUpGuildSkill.Request Parse(string s, NShared.LevelUpGuildSkill.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.LevelUpGuildSkill.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.LevelUpGuildSkill.Request());
	}

	public static NShared.LevelUpGuildSkill.Request Parse(IDictionary d, NShared.LevelUpGuildSkill.Request o)
	{
		o.SkillIndex = BaseJsonMarshaler.ParseValueInt(d, "SkillIndex");
		o.SkillLevel = BaseJsonMarshaler.ParseValueByte(d, "SkillLevel");
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.LevelUpGuildSkill.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.LevelUpGuildSkill.Request o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("SkillIndex", o.SkillIndex.ToString());
		dictionary.Add("SkillLevel", o.SkillLevel.ToString());
		if (o.SessionId != null)
		{
			dictionary.Add("SessionId", o.SessionId);
		}
		return dictionary;
	}
}
