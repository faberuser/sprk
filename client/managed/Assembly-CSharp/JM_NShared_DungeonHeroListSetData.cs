using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_DungeonHeroListSetData
{
	public static DungeonHeroListSetData Parse(string s)
	{
		return Parse(s, new DungeonHeroListSetData());
	}

	public static DungeonHeroListSetData Parse(string s, DungeonHeroListSetData o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static DungeonHeroListSetData Parse(IDictionary d)
	{
		return Parse(d, new DungeonHeroListSetData());
	}

	public static DungeonHeroListSetData Parse(IDictionary d, DungeonHeroListSetData o)
	{
		o.GroupIndex = BaseJsonMarshaler.ParseValueInt(d, "GroupIndex");
		o.HeroIndex = BaseJsonMarshaler.ParseValueInt(d, "HeroIndex");
		o.CostumeIndex = BaseJsonMarshaler.ParseValueInt(d, "CostumeIndex");
		return o;
	}

	public static string EncodeJsonString(DungeonHeroListSetData o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(DungeonHeroListSetData o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("GroupIndex", o.GroupIndex.ToString());
		dictionary.Add("HeroIndex", o.HeroIndex.ToString());
		dictionary.Add("CostumeIndex", o.CostumeIndex.ToString());
		return dictionary;
	}
}
