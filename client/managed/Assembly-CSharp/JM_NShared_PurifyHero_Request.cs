using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.PurifyHero;

public static class JM_NShared_PurifyHero_Request
{
	public static NShared.PurifyHero.Request Parse(string s)
	{
		return Parse(s, new NShared.PurifyHero.Request());
	}

	public static NShared.PurifyHero.Request Parse(string s, NShared.PurifyHero.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.PurifyHero.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.PurifyHero.Request());
	}

	public static NShared.PurifyHero.Request Parse(IDictionary d, NShared.PurifyHero.Request o)
	{
		o.HeroIndex = BaseJsonMarshaler.ParseValueInt(d, "HeroIndex");
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.PurifyHero.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.PurifyHero.Request o)
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
