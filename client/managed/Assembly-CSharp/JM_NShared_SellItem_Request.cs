using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.SellItem;

public static class JM_NShared_SellItem_Request
{
	public static NShared.SellItem.Request Parse(string s)
	{
		return Parse(s, new NShared.SellItem.Request());
	}

	public static NShared.SellItem.Request Parse(string s, NShared.SellItem.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.SellItem.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.SellItem.Request());
	}

	public static NShared.SellItem.Request Parse(IDictionary d, NShared.SellItem.Request o)
	{
		o.ItemIndices = BaseJsonMarshaler.ParseArrayCast(d, "ItemIndices", BaseJsonMarshaler.CastToInt);
		o.ItemCount = BaseJsonMarshaler.ParseArrayCast(d, "ItemCount", BaseJsonMarshaler.CastToInt);
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.SellItem.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.SellItem.Request o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("ItemIndices", BaseJsonMarshaler.EncodeArray(o.ItemIndices));
		dictionary.Add("ItemCount", BaseJsonMarshaler.EncodeArray(o.ItemCount));
		if (o.SessionId != null)
		{
			dictionary.Add("SessionId", o.SessionId);
		}
		return dictionary;
	}
}
