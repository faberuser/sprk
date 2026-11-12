using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.RequestLevelUpGuildSkill;

public static class JM_NShared_RequestLevelUpGuildSkill_Request
{
	public static NShared.RequestLevelUpGuildSkill.Request Parse(string s)
	{
		return Parse(s, new NShared.RequestLevelUpGuildSkill.Request());
	}

	public static NShared.RequestLevelUpGuildSkill.Request Parse(string s, NShared.RequestLevelUpGuildSkill.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.RequestLevelUpGuildSkill.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.RequestLevelUpGuildSkill.Request());
	}

	public static NShared.RequestLevelUpGuildSkill.Request Parse(IDictionary d, NShared.RequestLevelUpGuildSkill.Request o)
	{
		o.GuildSkillIndex = BaseJsonMarshaler.ParseValueByte(d, "GuildSkillIndex");
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.RequestLevelUpGuildSkill.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.RequestLevelUpGuildSkill.Request o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("GuildSkillIndex", o.GuildSkillIndex.ToString());
		if (o.SessionId != null)
		{
			dictionary.Add("SessionId", o.SessionId);
		}
		return dictionary;
	}
}
