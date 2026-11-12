using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.ResetHeroTranscendSkillPage;

public static class JM_NShared_ResetHeroTranscendSkillPage_Request
{
	public static NShared.ResetHeroTranscendSkillPage.Request Parse(string s)
	{
		return Parse(s, new NShared.ResetHeroTranscendSkillPage.Request());
	}

	public static NShared.ResetHeroTranscendSkillPage.Request Parse(string s, NShared.ResetHeroTranscendSkillPage.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.ResetHeroTranscendSkillPage.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.ResetHeroTranscendSkillPage.Request());
	}

	public static NShared.ResetHeroTranscendSkillPage.Request Parse(IDictionary d, NShared.ResetHeroTranscendSkillPage.Request o)
	{
		o.HeroIndex = BaseJsonMarshaler.ParseValueInt(d, "HeroIndex");
		o.PageIndex = BaseJsonMarshaler.ParseValueInt(d, "PageIndex");
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.ResetHeroTranscendSkillPage.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.ResetHeroTranscendSkillPage.Request o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("HeroIndex", o.HeroIndex.ToString());
		dictionary.Add("PageIndex", o.PageIndex.ToString());
		if (o.SessionId != null)
		{
			dictionary.Add("SessionId", o.SessionId);
		}
		return dictionary;
	}
}
