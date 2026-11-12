using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_ItemInfo
{
	public static ItemInfo Parse(string s)
	{
		return Parse(s, new ItemInfo());
	}

	public static ItemInfo Parse(string s, ItemInfo o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static ItemInfo Parse(IDictionary d)
	{
		return Parse(d, new ItemInfo());
	}

	public static ItemInfo Parse(IDictionary d, ItemInfo o)
	{
		o.ItemIndex = BaseJsonMarshaler.ParseValueInt(d, "ItemIndex");
		o.Count = BaseJsonMarshaler.ParseValueInt(d, "Count");
		o.CreatedTime = BaseJsonMarshaler.ParseValueString(d, "CreatedTime");
		o.Uid = BaseJsonMarshaler.ParseValueString(d, "Uid");
		o.Locked = BaseJsonMarshaler.ParseValueByte(d, "Locked");
		return o;
	}

	public static string EncodeJsonString(ItemInfo o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(ItemInfo o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("ItemIndex", o.ItemIndex.ToString());
		dictionary.Add("Count", o.Count.ToString());
		if (o.CreatedTime != null)
		{
			dictionary.Add("CreatedTime", o.CreatedTime);
		}
		if (o.Uid != null)
		{
			dictionary.Add("Uid", o.Uid);
		}
		dictionary.Add("Locked", o.Locked.ToString());
		return dictionary;
	}
}
