using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_HeroPresetStorageInfo
{
	public static HeroPresetStorageInfo Parse(string s)
	{
		return Parse(s, new HeroPresetStorageInfo());
	}

	public static HeroPresetStorageInfo Parse(string s, HeroPresetStorageInfo o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static HeroPresetStorageInfo Parse(IDictionary d)
	{
		return Parse(d, new HeroPresetStorageInfo());
	}

	public static HeroPresetStorageInfo Parse(IDictionary d, HeroPresetStorageInfo o)
	{
		o.StorageKey = BaseJsonMarshaler.ParseValueString(d, "StorageKey");
		o.Name = BaseJsonMarshaler.ParseValueString(d, "Name");
		o.HeroPresetInfos = BaseJsonMarshaler.ParseArray(d, "HeroPresetInfos", JM_NShared_HeroPresetInfo.Parse);
		return o;
	}

	public static string EncodeJsonString(HeroPresetStorageInfo o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(HeroPresetStorageInfo o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		if (o.StorageKey != null)
		{
			dictionary.Add("StorageKey", o.StorageKey);
		}
		if (o.Name != null)
		{
			dictionary.Add("Name", o.Name);
		}
		dictionary.Add("HeroPresetInfos", BaseJsonMarshaler.EncodeArray(o.HeroPresetInfos, JM_NShared_HeroPresetInfo.EncodeJsonObject));
		return dictionary;
	}
}
