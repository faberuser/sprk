using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_ItemBoosterResultInfo
{
	public static ItemBoosterResultInfo Parse(string s)
	{
		return Parse(s, new ItemBoosterResultInfo());
	}

	public static ItemBoosterResultInfo Parse(string s, ItemBoosterResultInfo o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static ItemBoosterResultInfo Parse(IDictionary d)
	{
		return Parse(d, new ItemBoosterResultInfo());
	}

	public static ItemBoosterResultInfo Parse(IDictionary d, ItemBoosterResultInfo o)
	{
		o.RemainCount = BaseJsonMarshaler.ParseValueInt(d, "RemainCount");
		o.MaxCount = BaseJsonMarshaler.ParseValueInt(d, "MaxCount");
		return o;
	}

	public static string EncodeJsonString(ItemBoosterResultInfo o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(ItemBoosterResultInfo o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("RemainCount", o.RemainCount.ToString());
		dictionary.Add("MaxCount", o.MaxCount.ToString());
		return dictionary;
	}
}
