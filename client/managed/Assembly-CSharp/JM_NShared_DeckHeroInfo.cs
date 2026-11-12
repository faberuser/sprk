using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_DeckHeroInfo
{
	public static DeckHeroInfo Parse(string s)
	{
		return Parse(s, new DeckHeroInfo());
	}

	public static DeckHeroInfo Parse(string s, DeckHeroInfo o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static DeckHeroInfo Parse(IDictionary d)
	{
		return Parse(d, new DeckHeroInfo());
	}

	public static DeckHeroInfo Parse(IDictionary d, DeckHeroInfo o)
	{
		o.Index = BaseJsonMarshaler.ParseValueInt(d, "Index");
		o.Star = BaseJsonMarshaler.ParseValueInt(d, "Star");
		o.Level = BaseJsonMarshaler.ParseValueInt(d, "Level");
		return o;
	}

	public static string EncodeJsonString(DeckHeroInfo o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(DeckHeroInfo o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("Index", o.Index.ToString());
		dictionary.Add("Star", o.Star.ToString());
		dictionary.Add("Level", o.Level.ToString());
		return dictionary;
	}
}
