using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.InstantCraftItem;

public static class JM_NShared_InstantCraftItem_Request
{
	public static NShared.InstantCraftItem.Request Parse(string s)
	{
		return Parse(s, new NShared.InstantCraftItem.Request());
	}

	public static NShared.InstantCraftItem.Request Parse(string s, NShared.InstantCraftItem.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.InstantCraftItem.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.InstantCraftItem.Request());
	}

	public static NShared.InstantCraftItem.Request Parse(IDictionary d, NShared.InstantCraftItem.Request o)
	{
		o.SlotIndex = BaseJsonMarshaler.ParseValueInt(d, "SlotIndex");
		o.RemainTime = BaseJsonMarshaler.ParseValueInt(d, "RemainTime");
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.InstantCraftItem.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.InstantCraftItem.Request o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("SlotIndex", o.SlotIndex.ToString());
		dictionary.Add("RemainTime", o.RemainTime.ToString());
		if (o.SessionId != null)
		{
			dictionary.Add("SessionId", o.SessionId);
		}
		return dictionary;
	}
}
