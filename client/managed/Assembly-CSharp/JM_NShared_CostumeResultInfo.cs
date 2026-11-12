using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_CostumeResultInfo
{
	public static CostumeResultInfo Parse(string s)
	{
		return Parse(s, new CostumeResultInfo());
	}

	public static CostumeResultInfo Parse(string s, CostumeResultInfo o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static CostumeResultInfo Parse(IDictionary d)
	{
		return Parse(d, new CostumeResultInfo());
	}

	public static CostumeResultInfo Parse(IDictionary d, CostumeResultInfo o)
	{
		o.HeroIndex = BaseJsonMarshaler.ParseValueInt(d, "HeroIndex");
		o.CostumeIndex = BaseJsonMarshaler.ParseValueInt(d, "CostumeIndex");
		return o;
	}

	public static string EncodeJsonString(CostumeResultInfo o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(CostumeResultInfo o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("HeroIndex", o.HeroIndex.ToString());
		dictionary.Add("CostumeIndex", o.CostumeIndex.ToString());
		return dictionary;
	}
}
