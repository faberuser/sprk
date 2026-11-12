using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.TranscendHero;

public static class JM_NShared_TranscendHero_Request
{
	public static NShared.TranscendHero.Request Parse(string s)
	{
		return Parse(s, new NShared.TranscendHero.Request());
	}

	public static NShared.TranscendHero.Request Parse(string s, NShared.TranscendHero.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.TranscendHero.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.TranscendHero.Request());
	}

	public static NShared.TranscendHero.Request Parse(IDictionary d, NShared.TranscendHero.Request o)
	{
		o.HeroIndex = BaseJsonMarshaler.ParseValueInt(d, "HeroIndex");
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.TranscendHero.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.TranscendHero.Request o)
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
