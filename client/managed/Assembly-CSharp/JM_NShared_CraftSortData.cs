using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_CraftSortData
{
	public static CraftSortData Parse(string s)
	{
		return Parse(s, new CraftSortData());
	}

	public static CraftSortData Parse(string s, CraftSortData o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static CraftSortData Parse(IDictionary d)
	{
		return Parse(d, new CraftSortData());
	}

	public static CraftSortData Parse(IDictionary d, CraftSortData o)
	{
		o.Category = BaseJsonMarshaler.ParseEnum<CraftSortCategory>(d, "Category");
		o.Value = BaseJsonMarshaler.ParseValueInt(d, "Value");
		o.BackGroundIndex = BaseJsonMarshaler.ParseValueInt(d, "BackGroundIndex");
		o.StringKey = BaseJsonMarshaler.ParseValueString(d, "StringKey");
		return o;
	}

	public static string EncodeJsonString(CraftSortData o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(CraftSortData o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("Category", o.Category.ToString());
		dictionary.Add("Value", o.Value.ToString());
		dictionary.Add("BackGroundIndex", o.BackGroundIndex.ToString());
		if (o.StringKey != null)
		{
			dictionary.Add("StringKey", o.StringKey);
		}
		return dictionary;
	}
}
