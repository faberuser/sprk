using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_CraftSlotResultInfo
{
	public static CraftSlotResultInfo Parse(string s)
	{
		return Parse(s, new CraftSlotResultInfo());
	}

	public static CraftSlotResultInfo Parse(string s, CraftSlotResultInfo o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static CraftSlotResultInfo Parse(IDictionary d)
	{
		return Parse(d, new CraftSlotResultInfo());
	}

	public static CraftSlotResultInfo Parse(IDictionary d, CraftSlotResultInfo o)
	{
		o.SlotIndex = BaseJsonMarshaler.ParseValueInt(d, "SlotIndex");
		o.CraftIndex = BaseJsonMarshaler.ParseValueInt(d, "CraftIndex");
		o.ItemIndex = BaseJsonMarshaler.ParseValueInt(d, "ItemIndex");
		o.RemainTime = BaseJsonMarshaler.ParseValueInt(d, "RemainTime");
		o.ItemCount = BaseJsonMarshaler.ParseValueInt(d, "ItemCount");
		return o;
	}

	public static string EncodeJsonString(CraftSlotResultInfo o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(CraftSlotResultInfo o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("SlotIndex", o.SlotIndex.ToString());
		dictionary.Add("CraftIndex", o.CraftIndex.ToString());
		dictionary.Add("ItemIndex", o.ItemIndex.ToString());
		dictionary.Add("RemainTime", o.RemainTime.ToString());
		dictionary.Add("ItemCount", o.ItemCount.ToString());
		return dictionary;
	}
}
