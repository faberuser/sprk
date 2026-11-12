using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_HairCostumeInfo
{
	public static HairCostumeInfo Parse(string s)
	{
		return Parse(s, new HairCostumeInfo());
	}

	public static HairCostumeInfo Parse(string s, HairCostumeInfo o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static HairCostumeInfo Parse(IDictionary d)
	{
		return Parse(d, new HairCostumeInfo());
	}

	public static HairCostumeInfo Parse(IDictionary d, HairCostumeInfo o)
	{
		o.HairCostumeIndex = BaseJsonMarshaler.ParseValueInt(d, "HairCostumeIndex");
		o.CreatedTime = BaseJsonMarshaler.ParseValueString(d, "CreatedTime");
		return o;
	}

	public static string EncodeJsonString(HairCostumeInfo o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(HairCostumeInfo o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("HairCostumeIndex", o.HairCostumeIndex.ToString());
		if (o.CreatedTime != null)
		{
			dictionary.Add("CreatedTime", o.CreatedTime);
		}
		return dictionary;
	}
}
