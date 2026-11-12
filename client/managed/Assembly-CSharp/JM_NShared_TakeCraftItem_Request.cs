using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.TakeCraftItem;

public static class JM_NShared_TakeCraftItem_Request
{
	public static NShared.TakeCraftItem.Request Parse(string s)
	{
		return Parse(s, new NShared.TakeCraftItem.Request());
	}

	public static NShared.TakeCraftItem.Request Parse(string s, NShared.TakeCraftItem.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.TakeCraftItem.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.TakeCraftItem.Request());
	}

	public static NShared.TakeCraftItem.Request Parse(IDictionary d, NShared.TakeCraftItem.Request o)
	{
		o.SlotIndex = BaseJsonMarshaler.ParseValueInt(d, "SlotIndex");
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.TakeCraftItem.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.TakeCraftItem.Request o)
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
