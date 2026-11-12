using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_PlayerAnyMisc
{
	public static PlayerAnyMisc Parse(string s)
	{
		return Parse(s, new PlayerAnyMisc());
	}

	public static PlayerAnyMisc Parse(string s, PlayerAnyMisc o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static PlayerAnyMisc Parse(IDictionary d)
	{
		return Parse(d, new PlayerAnyMisc());
	}

	public static PlayerAnyMisc Parse(IDictionary d, PlayerAnyMisc o)
	{
		o.MiscKey = BaseJsonMarshaler.ParseValueString(d, "MiscKey");
		o.MiscValue = BaseJsonMarshaler.ParseValueString(d, "MiscValue");
		o.EndTime = BaseJsonMarshaler.ParseValueString(d, "EndTime");
		return o;
	}

	public static string EncodeJsonString(PlayerAnyMisc o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(PlayerAnyMisc o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		if (o.MiscKey != null)
		{
			dictionary.Add("MiscKey", o.MiscKey);
		}
		if (o.MiscValue != null)
		{
			dictionary.Add("MiscValue", o.MiscValue);
		}
		if (o.EndTime != null)
		{
			dictionary.Add("EndTime", o.EndTime);
		}
		return dictionary;
	}
}
