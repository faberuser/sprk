using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_IndexedRewardItemInfo
{
	public static IndexedRewardItemInfo Parse(string s)
	{
		return Parse(s, new IndexedRewardItemInfo());
	}

	public static IndexedRewardItemInfo Parse(string s, IndexedRewardItemInfo o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static IndexedRewardItemInfo Parse(IDictionary d)
	{
		return Parse(d, new IndexedRewardItemInfo());
	}

	public static IndexedRewardItemInfo Parse(IDictionary d, IndexedRewardItemInfo o)
	{
		o.ItemIndex = BaseJsonMarshaler.ParseValueInt(d, "ItemIndex");
		o.Star = BaseJsonMarshaler.ParseValueInt(d, "Star");
		o.Count = BaseJsonMarshaler.ParseValueInt(d, "Count");
		return o;
	}

	public static string EncodeJsonString(IndexedRewardItemInfo o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(IndexedRewardItemInfo o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("ItemIndex", o.ItemIndex.ToString());
		dictionary.Add("Star", o.Star.ToString());
		dictionary.Add("Count", o.Count.ToString());
		return dictionary;
	}
}
