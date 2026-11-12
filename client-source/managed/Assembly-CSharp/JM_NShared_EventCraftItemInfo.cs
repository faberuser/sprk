using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_EventCraftItemInfo
{
	public static EventCraftItemInfo Parse(string s)
	{
		return Parse(s, new EventCraftItemInfo());
	}

	public static EventCraftItemInfo Parse(string s, EventCraftItemInfo o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static EventCraftItemInfo Parse(IDictionary d)
	{
		return Parse(d, new EventCraftItemInfo());
	}

	public static EventCraftItemInfo Parse(IDictionary d, EventCraftItemInfo o)
	{
		o.Index = BaseJsonMarshaler.ParseValueInt(d, "Index");
		o.ShowGroup = BaseJsonMarshaler.ParseValueString(d, "ShowGroup");
		o.CraftCount = BaseJsonMarshaler.ParseValueInt(d, "CraftCount");
		o.UpdatedTime = BaseJsonMarshaler.ParseValueString(d, "UpdatedTime");
		return o;
	}

	public static string EncodeJsonString(EventCraftItemInfo o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(EventCraftItemInfo o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("Index", o.Index.ToString());
		if (o.ShowGroup != null)
		{
			dictionary.Add("ShowGroup", o.ShowGroup);
		}
		dictionary.Add("CraftCount", o.CraftCount.ToString());
		if (o.UpdatedTime != null)
		{
			dictionary.Add("UpdatedTime", o.UpdatedTime);
		}
		return dictionary;
	}
}
