using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.ResurrectionHero;

public static class JM_NShared_ResurrectionHero_Request
{
	public static NShared.ResurrectionHero.Request Parse(string s)
	{
		return Parse(s, new NShared.ResurrectionHero.Request());
	}

	public static NShared.ResurrectionHero.Request Parse(string s, NShared.ResurrectionHero.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.ResurrectionHero.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.ResurrectionHero.Request());
	}

	public static NShared.ResurrectionHero.Request Parse(IDictionary d, NShared.ResurrectionHero.Request o)
	{
		o.HeroIndex = BaseJsonMarshaler.ParseValueInt(d, "HeroIndex");
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.ResurrectionHero.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.ResurrectionHero.Request o)
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
