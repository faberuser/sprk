using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_CraftSlotInfo
{
	public static CraftSlotInfo Parse(string s)
	{
		return Parse(s, new CraftSlotInfo());
	}

	public static CraftSlotInfo Parse(string s, CraftSlotInfo o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static CraftSlotInfo Parse(IDictionary d)
	{
		return Parse(d, new CraftSlotInfo());
	}

	public static CraftSlotInfo Parse(IDictionary d, CraftSlotInfo o)
	{
		o.SlotIndex = BaseJsonMarshaler.ParseValueInt(d, "SlotIndex");
		o.CraftIndex = BaseJsonMarshaler.ParseValueInt(d, "CraftIndex");
		o.ItemIndex = BaseJsonMarshaler.ParseValueInt(d, "ItemIndex");
		o.CompleteTime = BaseJsonMarshaler.ParseValueString(d, "CompleteTime");
		o.ItemCount = BaseJsonMarshaler.ParseValueInt(d, "ItemCount");
		return o;
	}

	public static string EncodeJsonString(CraftSlotInfo o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(CraftSlotInfo o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("SlotIndex", o.SlotIndex.ToString());
		dictionary.Add("CraftIndex", o.CraftIndex.ToString());
		dictionary.Add("ItemIndex", o.ItemIndex.ToString());
		if (o.CompleteTime != null)
		{
			dictionary.Add("CompleteTime", o.CompleteTime);
		}
		dictionary.Add("ItemCount", o.ItemCount.ToString());
		return dictionary;
	}
}
