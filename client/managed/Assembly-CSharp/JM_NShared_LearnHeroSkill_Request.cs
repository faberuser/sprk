using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.LearnHeroSkill;

public static class JM_NShared_LearnHeroSkill_Request
{
	public static NShared.LearnHeroSkill.Request Parse(string s)
	{
		return Parse(s, new NShared.LearnHeroSkill.Request());
	}

	public static NShared.LearnHeroSkill.Request Parse(string s, NShared.LearnHeroSkill.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.LearnHeroSkill.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.LearnHeroSkill.Request());
	}

	public static NShared.LearnHeroSkill.Request Parse(IDictionary d, NShared.LearnHeroSkill.Request o)
	{
		o.HeroIndex = BaseJsonMarshaler.ParseValueInt(d, "HeroIndex");
		o.SkillIndex = BaseJsonMarshaler.ParseValueInt(d, "SkillIndex");
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.LearnHeroSkill.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.LearnHeroSkill.Request o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("HeroIndex", o.HeroIndex.ToString());
		dictionary.Add("SkillIndex", o.SkillIndex.ToString());
		if (o.SessionId != null)
		{
			dictionary.Add("SessionId", o.SessionId);
		}
		return dictionary;
	}
}
