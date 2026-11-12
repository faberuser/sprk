using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_WeaponCostumeInfo
{
	public static WeaponCostumeInfo Parse(string s)
	{
		return Parse(s, new WeaponCostumeInfo());
	}

	public static WeaponCostumeInfo Parse(string s, WeaponCostumeInfo o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static WeaponCostumeInfo Parse(IDictionary d)
	{
		return Parse(d, new WeaponCostumeInfo());
	}

	public static WeaponCostumeInfo Parse(IDictionary d, WeaponCostumeInfo o)
	{
		o.WeaponCostumeIndex = BaseJsonMarshaler.ParseValueInt(d, "WeaponCostumeIndex");
		o.CreatedTime = BaseJsonMarshaler.ParseValueString(d, "CreatedTime");
		return o;
	}

	public static string EncodeJsonString(WeaponCostumeInfo o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(WeaponCostumeInfo o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("WeaponCostumeIndex", o.WeaponCostumeIndex.ToString());
		if (o.CreatedTime != null)
		{
			dictionary.Add("CreatedTime", o.CreatedTime);
		}
		return dictionary;
	}
}
