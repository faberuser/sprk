using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.ResetHeroTranscendSkill;

public static class JM_NShared_ResetHeroTranscendSkill_Request
{
	public static NShared.ResetHeroTranscendSkill.Request Parse(string s)
	{
		return Parse(s, new NShared.ResetHeroTranscendSkill.Request());
	}

	public static NShared.ResetHeroTranscendSkill.Request Parse(string s, NShared.ResetHeroTranscendSkill.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.ResetHeroTranscendSkill.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.ResetHeroTranscendSkill.Request());
	}

	public static NShared.ResetHeroTranscendSkill.Request Parse(IDictionary d, NShared.ResetHeroTranscendSkill.Request o)
	{
		o.HeroIndex = BaseJsonMarshaler.ParseValueInt(d, "HeroIndex");
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.ResetHeroTranscendSkill.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.ResetHeroTranscendSkill.Request o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("HeroIndex", o.HeroIndex.ToString());
		if (o.SessionId != null)
		{
			dictionary.Add("SessionId", o.SessionId);
		}
		return dictionary;
	}
}
