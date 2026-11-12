using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_ItemUseResultInfo
{
	public static ItemUseResultInfo Parse(string s)
	{
		return Parse(s, new ItemUseResultInfo());
	}

	public static ItemUseResultInfo Parse(string s, ItemUseResultInfo o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static ItemUseResultInfo Parse(IDictionary d)
	{
		return Parse(d, new ItemUseResultInfo());
	}

	public static ItemUseResultInfo Parse(IDictionary d, ItemUseResultInfo o)
	{
		o.ItemIndex = BaseJsonMarshaler.ParseValueInt(d, "ItemIndex");
		o.OutItemIndex = BaseJsonMarshaler.ParseValueInt(d, "OutItemIndex");
		o.AddCount = BaseJsonMarshaler.ParseValueInt(d, "AddCount");
		o.NewCount = BaseJsonMarshaler.ParseValueInt(d, "NewCount");
		o.CompletedTime = BaseJsonMarshaler.ParseValueString(d, "CompletedTime");
		return o;
	}

	public static string EncodeJsonString(ItemUseResultInfo o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(ItemUseResultInfo o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("ItemIndex", o.ItemIndex.ToString());
		dictionary.Add("OutItemIndex", o.OutItemIndex.ToString());
		dictionary.Add("AddCount", o.AddCount.ToString());
		dictionary.Add("NewCount", o.NewCount.ToString());
		if (o.CompletedTime != null)
		{
			dictionary.Add("CompletedTime", o.CompletedTime);
		}
		return dictionary;
	}
}
