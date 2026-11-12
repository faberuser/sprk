using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_PlayerAccessoryCostumeInfo
{
	public static PlayerAccessoryCostumeInfo Parse(string s)
	{
		return Parse(s, new PlayerAccessoryCostumeInfo());
	}

	public static PlayerAccessoryCostumeInfo Parse(string s, PlayerAccessoryCostumeInfo o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static PlayerAccessoryCostumeInfo Parse(IDictionary d)
	{
		return Parse(d, new PlayerAccessoryCostumeInfo());
	}

	public static PlayerAccessoryCostumeInfo Parse(IDictionary d, PlayerAccessoryCostumeInfo o)
	{
		o.HeroIndex = BaseJsonMarshaler.ParseValueInt(d, "HeroIndex");
		o.AccessoryCostumeIndex = BaseJsonMarshaler.ParseValueInt(d, "AccessoryCostumeIndex");
		o.PositionInfo = BaseJsonMarshaler.ParseValueString(d, "PositionInfo");
		o.CreatedTime = BaseJsonMarshaler.ParseValueString(d, "CreatedTime");
		return o;
	}

	public static string EncodeJsonString(PlayerAccessoryCostumeInfo o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(PlayerAccessoryCostumeInfo o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("HeroIndex", o.HeroIndex.ToString());
		dictionary.Add("AccessoryCostumeIndex", o.AccessoryCostumeIndex.ToString());
		if (o.PositionInfo != null)
		{
			dictionary.Add("PositionInfo", o.PositionInfo);
		}
		if (o.CreatedTime != null)
		{
			dictionary.Add("CreatedTime", o.CreatedTime);
		}
		return dictionary;
	}
}
