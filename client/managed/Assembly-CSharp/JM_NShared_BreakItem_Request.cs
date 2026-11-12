using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.BreakItem;

public static class JM_NShared_BreakItem_Request
{
	public static NShared.BreakItem.Request Parse(string s)
	{
		return Parse(s, new NShared.BreakItem.Request());
	}

	public static NShared.BreakItem.Request Parse(string s, NShared.BreakItem.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.BreakItem.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.BreakItem.Request());
	}

	public static NShared.BreakItem.Request Parse(IDictionary d, NShared.BreakItem.Request o)
	{
		o.ItemIndices = BaseJsonMarshaler.ParseArrayCast(d, "ItemIndices", BaseJsonMarshaler.CastToInt);
		o.Counts = BaseJsonMarshaler.ParseArrayCast(d, "Counts", BaseJsonMarshaler.CastToInt);
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.BreakItem.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.BreakItem.Request o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("ItemIndices", BaseJsonMarshaler.EncodeArray(o.ItemIndices));
		dictionary.Add("Counts", BaseJsonMarshaler.EncodeArray(o.Counts));
		if (o.SessionId != null)
		{
			dictionary.Add("SessionId", o.SessionId);
		}
		return dictionary;
	}
}
