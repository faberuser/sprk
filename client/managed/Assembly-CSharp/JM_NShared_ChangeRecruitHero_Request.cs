using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.ChangeRecruitHero;

public static class JM_NShared_ChangeRecruitHero_Request
{
	public static NShared.ChangeRecruitHero.Request Parse(string s)
	{
		return Parse(s, new NShared.ChangeRecruitHero.Request());
	}

	public static NShared.ChangeRecruitHero.Request Parse(string s, NShared.ChangeRecruitHero.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.ChangeRecruitHero.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.ChangeRecruitHero.Request());
	}

	public static NShared.ChangeRecruitHero.Request Parse(IDictionary d, NShared.ChangeRecruitHero.Request o)
	{
		o.HeroIndex = BaseJsonMarshaler.ParseValueInt(d, "HeroIndex");
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.ChangeRecruitHero.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.ChangeRecruitHero.Request o)
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
