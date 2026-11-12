using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.CancelCraftItem;

public static class JM_NShared_CancelCraftItem_Request
{
	public static NShared.CancelCraftItem.Request Parse(string s)
	{
		return Parse(s, new NShared.CancelCraftItem.Request());
	}

	public static NShared.CancelCraftItem.Request Parse(string s, NShared.CancelCraftItem.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.CancelCraftItem.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.CancelCraftItem.Request());
	}

	public static NShared.CancelCraftItem.Request Parse(IDictionary d, NShared.CancelCraftItem.Request o)
	{
		o.SlotIndex = BaseJsonMarshaler.ParseValueInt(d, "SlotIndex");
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.CancelCraftItem.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.CancelCraftItem.Request o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("SlotIndex", o.SlotIndex.ToString());
		if (o.SessionId != null)
		{
			dictionary.Add("SessionId", o.SessionId);
		}
		return dictionary;
	}
}
