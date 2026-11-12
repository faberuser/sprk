using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.BookmarkHero;

public static class JM_NShared_BookmarkHero_Request
{
	public static NShared.BookmarkHero.Request Parse(string s)
	{
		return Parse(s, new NShared.BookmarkHero.Request());
	}

	public static NShared.BookmarkHero.Request Parse(string s, NShared.BookmarkHero.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.BookmarkHero.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.BookmarkHero.Request());
	}

	public static NShared.BookmarkHero.Request Parse(IDictionary d, NShared.BookmarkHero.Request o)
	{
		o.HeroIndex = BaseJsonMarshaler.ParseArrayCast(d, "HeroIndex", BaseJsonMarshaler.CastToInt);
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.BookmarkHero.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.BookmarkHero.Request o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("HeroIndex", BaseJsonMarshaler.EncodeArray(o.HeroIndex));
		if (o.SessionId != null)
		{
			dictionary.Add("SessionId", o.SessionId);
		}
		return dictionary;
	}
}
