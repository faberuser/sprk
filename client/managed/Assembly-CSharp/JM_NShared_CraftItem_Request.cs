using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.CraftItem;

public static class JM_NShared_CraftItem_Request
{
	public static NShared.CraftItem.Request Parse(string s)
	{
		return Parse(s, new NShared.CraftItem.Request());
	}

	public static NShared.CraftItem.Request Parse(string s, NShared.CraftItem.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.CraftItem.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.CraftItem.Request());
	}

	public static NShared.CraftItem.Request Parse(IDictionary d, NShared.CraftItem.Request o)
	{
		o.SlotIndex = BaseJsonMarshaler.ParseValueInt(d, "SlotIndex");
		o.CraftIndex = BaseJsonMarshaler.ParseValueInt(d, "CraftIndex");
		o.ItemIndex = BaseJsonMarshaler.ParseValueInt(d, "ItemIndex");
		o.ItemCount = BaseJsonMarshaler.ParseValueInt(d, "ItemCount");
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.CraftItem.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.CraftItem.Request o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("SlotIndex", o.SlotIndex.ToString());
		dictionary.Add("CraftIndex", o.CraftIndex.ToString());
		dictionary.Add("ItemIndex", o.ItemIndex.ToString());
		dictionary.Add("ItemCount", o.ItemCount.ToString());
		if (o.SessionId != null)
		{
			dictionary.Add("SessionId", o.SessionId);
		}
		return dictionary;
	}
}
