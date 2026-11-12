using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_PvpCoinResultInfo
{
	public static PvpCoinResultInfo Parse(string s)
	{
		return Parse(s, new PvpCoinResultInfo());
	}

	public static PvpCoinResultInfo Parse(string s, PvpCoinResultInfo o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static PvpCoinResultInfo Parse(IDictionary d)
	{
		return Parse(d, new PvpCoinResultInfo());
	}

	public static PvpCoinResultInfo Parse(IDictionary d, PvpCoinResultInfo o)
	{
		o.AddValue = BaseJsonMarshaler.ParseValueLong(d, "AddValue");
		o.NewValue = BaseJsonMarshaler.ParseValueLong(d, "NewValue");
		return o;
	}

	public static string EncodeJsonString(PvpCoinResultInfo o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(PvpCoinResultInfo o)
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
