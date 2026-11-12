using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_RoyalPointResultInfo
{
	public static RoyalPointResultInfo Parse(string s)
	{
		return Parse(s, new RoyalPointResultInfo());
	}

	public static RoyalPointResultInfo Parse(string s, RoyalPointResultInfo o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static RoyalPointResultInfo Parse(IDictionary d)
	{
		return Parse(d, new RoyalPointResultInfo());
	}

	public static RoyalPointResultInfo Parse(IDictionary d, RoyalPointResultInfo o)
	{
		o.AddValue = BaseJsonMarshaler.ParseValueInt(d, "AddValue");
		o.NewValue = BaseJsonMarshaler.ParseValueLong(d, "NewValue");
		return o;
	}

	public static string EncodeJsonString(RoyalPointResultInfo o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(RoyalPointResultInfo o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("AddValue", o.AddValue.ToString());
		dictionary.Add("NewValue", o.NewValue.ToString());
		return dictionary;
	}
}
