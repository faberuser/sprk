using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.LearnHeroTranscendSkill;

public static class JM_NShared_LearnHeroTranscendSkill_Request
{
	public static NShared.LearnHeroTranscendSkill.Request Parse(string s)
	{
		return Parse(s, new NShared.LearnHeroTranscendSkill.Request());
	}

	public static NShared.LearnHeroTranscendSkill.Request Parse(string s, NShared.LearnHeroTranscendSkill.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.LearnHeroTranscendSkill.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.LearnHeroTranscendSkill.Request());
	}

	public static NShared.LearnHeroTranscendSkill.Request Parse(IDictionary d, NShared.LearnHeroTranscendSkill.Request o)
	{
		o.HeroIndex = BaseJsonMarshaler.ParseValueInt(d, "HeroIndex");
		o.TranscendSkill = BaseJsonMarshaler.ParseValueInt(d, "TranscendSkill");
		o.SkillIndex = BaseJsonMarshaler.ParseValueInt(d, "SkillIndex");
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.LearnHeroTranscendSkill.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.LearnHeroTranscendSkill.Request o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("HeroIndex", o.HeroIndex.ToString());
		dictionary.Add("TranscendSkill", o.TranscendSkill.ToString());
		dictionary.Add("SkillIndex", o.SkillIndex.ToString());
		if (o.SessionId != null)
		{
			dictionary.Add("SessionId", o.SessionId);
		}
		return dictionary;
	}
}
