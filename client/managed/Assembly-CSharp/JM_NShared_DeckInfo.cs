using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_DeckInfo
{
	public static DeckInfo Parse(string s)
	{
		return Parse(s, new DeckInfo());
	}

	public static DeckInfo Parse(string s, DeckInfo o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static DeckInfo Parse(IDictionary d)
	{
		return Parse(d, new DeckInfo());
	}

	public static DeckInfo Parse(IDictionary d, DeckInfo o)
	{
		o.BattleType = BaseJsonMarshaler.ParseEnum<BattleType>(d, "BattleType");
		o.HeroIndex1 = BaseJsonMarshaler.ParseValueInt(d, "HeroIndex1");
		o.HeroIndex2 = BaseJsonMarshaler.ParseValueInt(d, "HeroIndex2");
		o.HeroIndex3 = BaseJsonMarshaler.ParseValueInt(d, "HeroIndex3");
		o.HeroIndex4 = BaseJsonMarshaler.ParseValueInt(d, "HeroIndex4");
		o.ChapterIndex = BaseJsonMarshaler.ParseValueInt(d, "ChapterIndex");
		o.DungeonIndex = BaseJsonMarshaler.ParseValueInt(d, "DungeonIndex");
		o.PlayedTime = BaseJsonMarshaler.ParseValueString(d, "PlayedTime");
		return o;
	}

	public static string EncodeJsonString(DeckInfo o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(DeckInfo o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("BattleType", o.BattleType.ToString());
		dictionary.Add("HeroIndex1", o.HeroIndex1.ToString());
		dictionary.Add("HeroIndex2", o.HeroIndex2.ToString());
		dictionary.Add("HeroIndex3", o.HeroIndex3.ToString());
		dictionary.Add("HeroIndex4", o.HeroIndex4.ToString());
		dictionary.Add("ChapterIndex", o.ChapterIndex.ToString());
		dictionary.Add("DungeonIndex", o.DungeonIndex.ToString());
		if (o.PlayedTime != null)
		{
			dictionary.Add("PlayedTime", o.PlayedTime);
		}
		return dictionary;
	}
}
