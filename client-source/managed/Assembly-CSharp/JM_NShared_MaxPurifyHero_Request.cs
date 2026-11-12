using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.MaxPurifyHero;

public static class JM_NShared_MaxPurifyHero_Request
{
	public static NShared.MaxPurifyHero.Request Parse(string s)
	{
		return Parse(s, new NShared.MaxPurifyHero.Request());
	}

	public static NShared.MaxPurifyHero.Request Parse(string s, NShared.MaxPurifyHero.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.MaxPurifyHero.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.MaxPurifyHero.Request());
	}

	public static NShared.MaxPurifyHero.Request Parse(IDictionary d, NShared.MaxPurifyHero.Request o)
	{
		o.HeroIndex = BaseJsonMarshaler.ParseValueInt(d, "HeroIndex");
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.MaxPurifyHero.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.MaxPurifyHero.Request o)
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
