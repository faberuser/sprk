using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_CurrencyResultInfo2
{
	public static CurrencyResultInfo2 Parse(string s)
	{
		return Parse(s, new CurrencyResultInfo2());
	}

	public static CurrencyResultInfo2 Parse(string s, CurrencyResultInfo2 o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static CurrencyResultInfo2 Parse(IDictionary d)
	{
		return Parse(d, new CurrencyResultInfo2());
	}

	public static CurrencyResultInfo2 Parse(IDictionary d, CurrencyResultInfo2 o)
	{
		o.CurrencyType = BaseJsonMarshaler.ParseEnum<CurrencyType>(d, "CurrencyType");
		o.AddValue = BaseJsonMarshaler.ParseValueLong(d, "AddValue");
		o.NewValue = BaseJsonMarshaler.ParseValueLong(d, "NewValue");
		return o;
	}

	public static string EncodeJsonString(CurrencyResultInfo2 o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(CurrencyResultInfo2 o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("CurrencyType", o.CurrencyType.ToString());
		dictionary.Add("AddValue", o.AddValue.ToString());
		dictionary.Add("NewValue", o.NewValue.ToString());
		return dictionary;
	}
}
