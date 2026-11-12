using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_HeroStorageSlotInfo
{
	public static HeroStorageSlotInfo Parse(string s)
	{
		return Parse(s, new HeroStorageSlotInfo());
	}

	public static HeroStorageSlotInfo Parse(string s, HeroStorageSlotInfo o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static HeroStorageSlotInfo Parse(IDictionary d)
	{
		return Parse(d, new HeroStorageSlotInfo());
	}

	public static HeroStorageSlotInfo Parse(IDictionary d, HeroStorageSlotInfo o)
	{
		o.StorageKey = BaseJsonMarshaler.ParseValueString(d, "StorageKey");
		o.Name = BaseJsonMarshaler.ParseValueString(d, "Name");
		o.LinkInfo = BaseJsonMarshaler.ParseValueString(d, "LinkInfo");
		o.UpdatedTime = BaseJsonMarshaler.ParseValueString(d, "UpdatedTime");
		return o;
	}

	public static string EncodeJsonString(HeroStorageSlotInfo o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(HeroStorageSlotInfo o)
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
		if (o.LinkInfo != null)
		{
			dictionary.Add("LinkInfo", o.LinkInfo);
		}
		if (o.UpdatedTime != null)
		{
			dictionary.Add("UpdatedTime", o.UpdatedTime);
		}
		return dictionary;
	}
}
