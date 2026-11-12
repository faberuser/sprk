using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_CostumeInfo
{
	public static CostumeInfo Parse(string s)
	{
		return Parse(s, new CostumeInfo());
	}

	public static CostumeInfo Parse(string s, CostumeInfo o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static CostumeInfo Parse(IDictionary d)
	{
		return Parse(d, new CostumeInfo());
	}

	public static CostumeInfo Parse(IDictionary d, CostumeInfo o)
	{
		o.CostumeIndex = BaseJsonMarshaler.ParseValueInt(d, "CostumeIndex");
		o.ColorCostumeIndices = BaseJsonMarshaler.ParseArrayCast(d, "ColorCostumeIndices", BaseJsonMarshaler.CastToInt);
		o.CreatedTime = BaseJsonMarshaler.ParseValueString(d, "CreatedTime");
		return o;
	}

	public static string EncodeJsonString(CostumeInfo o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(CostumeInfo o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("CostumeIndex", o.CostumeIndex.ToString());
		dictionary.Add("ColorCostumeIndices", BaseJsonMarshaler.EncodeArray(o.ColorCostumeIndices));
		if (o.CreatedTime != null)
		{
			dictionary.Add("CreatedTime", o.CreatedTime);
		}
		return dictionary;
	}
}
