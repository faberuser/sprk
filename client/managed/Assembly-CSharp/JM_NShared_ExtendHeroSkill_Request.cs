using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.ExtendHeroSkill;

public static class JM_NShared_ExtendHeroSkill_Request
{
	public static NShared.ExtendHeroSkill.Request Parse(string s)
	{
		return Parse(s, new NShared.ExtendHeroSkill.Request());
	}

	public static NShared.ExtendHeroSkill.Request Parse(string s, NShared.ExtendHeroSkill.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.ExtendHeroSkill.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.ExtendHeroSkill.Request());
	}

	public static NShared.ExtendHeroSkill.Request Parse(IDictionary d, NShared.ExtendHeroSkill.Request o)
	{
		o.HeroIndex = BaseJsonMarshaler.ParseValueInt(d, "HeroIndex");
		o.SkillIndex = BaseJsonMarshaler.ParseValueInt(d, "SkillIndex");
		o.SkillExtend = BaseJsonMarshaler.ParseValueInt(d, "SkillExtend");
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.ExtendHeroSkill.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.ExtendHeroSkill.Request o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			SprkSkillUpgradeAll.Encode(dictionary, o);
			return dictionary;
		}
		dictionary.Add("HeroIndex", o.HeroIndex.ToString());
		dictionary.Add("SkillIndex", o.SkillIndex.ToString());
		dictionary.Add("SkillExtend", o.SkillExtend.ToString());
		if (o.SessionId != null)
		{
			dictionary.Add("SessionId", o.SessionId);
		}
		SprkSkillUpgradeAll.Encode(dictionary, o);
		return dictionary;
	}
}
