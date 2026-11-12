using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.UseBoosterItem;

public static class JM_NShared_UseBoosterItem_Request
{
	public static NShared.UseBoosterItem.Request Parse(string s)
	{
		return Parse(s, new NShared.UseBoosterItem.Request());
	}

	public static NShared.UseBoosterItem.Request Parse(string s, NShared.UseBoosterItem.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.UseBoosterItem.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.UseBoosterItem.Request());
	}

	public static NShared.UseBoosterItem.Request Parse(IDictionary d, NShared.UseBoosterItem.Request o)
	{
		o.ItemIndex = BaseJsonMarshaler.ParseValueInt(d, "ItemIndex");
		o.ItemCount = BaseJsonMarshaler.ParseValueInt(d, "ItemCount");
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.UseBoosterItem.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.UseBoosterItem.Request o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("ItemIndex", o.ItemIndex.ToString());
		dictionary.Add("ItemCount", o.ItemCount.ToString());
		if (o.SessionId != null)
		{
			dictionary.Add("SessionId", o.SessionId);
		}
		return dictionary;
	}
}
