using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.GiveGiftItem;

public static class JM_NShared_GiveGiftItem_Request
{
	public static NShared.GiveGiftItem.Request Parse(string s)
	{
		return Parse(s, new NShared.GiveGiftItem.Request());
	}

	public static NShared.GiveGiftItem.Request Parse(string s, NShared.GiveGiftItem.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.GiveGiftItem.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.GiveGiftItem.Request());
	}

	public static NShared.GiveGiftItem.Request Parse(IDictionary d, NShared.GiveGiftItem.Request o)
	{
		o.HeroIndex = BaseJsonMarshaler.ParseValueInt(d, "HeroIndex");
		o.ItemIndices = BaseJsonMarshaler.ParseArrayCast(d, "ItemIndices", BaseJsonMarshaler.CastToInt);
		o.ItemCount = BaseJsonMarshaler.ParseArrayCast(d, "ItemCount", BaseJsonMarshaler.CastToInt);
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.GiveGiftItem.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.GiveGiftItem.Request o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("HeroIndex", o.HeroIndex.ToString());
		dictionary.Add("ItemIndices", BaseJsonMarshaler.EncodeArray(o.ItemIndices));
		dictionary.Add("ItemCount", BaseJsonMarshaler.EncodeArray(o.ItemCount));
		if (o.SessionId != null)
		{
			dictionary.Add("SessionId", o.SessionId);
		}
		return dictionary;
	}
}
