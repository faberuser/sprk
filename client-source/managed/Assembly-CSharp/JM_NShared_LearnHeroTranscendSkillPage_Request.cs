using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.LearnHeroTranscendSkillPage;

public static class JM_NShared_LearnHeroTranscendSkillPage_Request
{
	public static NShared.LearnHeroTranscendSkillPage.Request Parse(string s)
	{
		return Parse(s, new NShared.LearnHeroTranscendSkillPage.Request());
	}

	public static NShared.LearnHeroTranscendSkillPage.Request Parse(string s, NShared.LearnHeroTranscendSkillPage.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.LearnHeroTranscendSkillPage.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.LearnHeroTranscendSkillPage.Request());
	}

	public static NShared.LearnHeroTranscendSkillPage.Request Parse(IDictionary d, NShared.LearnHeroTranscendSkillPage.Request o)
	{
		o.HeroIndex = BaseJsonMarshaler.ParseValueInt(d, "HeroIndex");
		o.TranscendSkills = BaseJsonMarshaler.ParseArrayCast(d, "TranscendSkills", BaseJsonMarshaler.CastToInt);
		o.PageIndex = BaseJsonMarshaler.ParseValueInt(d, "PageIndex");
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.LearnHeroTranscendSkillPage.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.LearnHeroTranscendSkillPage.Request o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("HeroIndex", o.HeroIndex.ToString());
		dictionary.Add("TranscendSkills", BaseJsonMarshaler.EncodeArray(o.TranscendSkills));
		dictionary.Add("PageIndex", o.PageIndex.ToString());
		if (o.SessionId != null)
		{
			dictionary.Add("SessionId", o.SessionId);
		}
		return dictionary;
	}
}
