using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.EventCraftItem;

public static class JM_NShared_EventCraftItem_Request
{
	public static NShared.EventCraftItem.Request Parse(string s)
	{
		return Parse(s, new NShared.EventCraftItem.Request());
	}

	public static NShared.EventCraftItem.Request Parse(string s, NShared.EventCraftItem.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.EventCraftItem.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.EventCraftItem.Request());
	}

	public static NShared.EventCraftItem.Request Parse(IDictionary d, NShared.EventCraftItem.Request o)
	{
		o.CraftIndex = BaseJsonMarshaler.ParseValueInt(d, "CraftIndex");
		o.CraftCount = BaseJsonMarshaler.ParseValueInt(d, "CraftCount");
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.EventCraftItem.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.EventCraftItem.Request o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("CraftIndex", o.CraftIndex.ToString());
		dictionary.Add("CraftCount", o.CraftCount.ToString());
		if (o.SessionId != null)
		{
			dictionary.Add("SessionId", o.SessionId);
		}
		return dictionary;
	}
}
