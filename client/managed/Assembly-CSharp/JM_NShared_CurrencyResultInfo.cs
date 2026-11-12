using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_CurrencyResultInfo
{
	public static CurrencyResultInfo Parse(string s)
	{
		return Parse(s, new CurrencyResultInfo());
	}

	public static CurrencyResultInfo Parse(string s, CurrencyResultInfo o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static CurrencyResultInfo Parse(IDictionary d)
	{
		return Parse(d, new CurrencyResultInfo());
	}

	public static CurrencyResultInfo Parse(IDictionary d, CurrencyResultInfo o)
	{
		o.CurrencyType = BaseJsonMarshaler.ParseEnum<RewardType>(d, "CurrencyType");
		o.ResultString = BaseJsonMarshaler.ParseValueString(d, "ResultString");
		return o;
	}

	public static string EncodeJsonString(CurrencyResultInfo o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(CurrencyResultInfo o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("CurrencyType", o.CurrencyType.ToString());
		if (o.ResultString != null)
		{
			dictionary.Add("ResultString", o.ResultString);
		}
		return dictionary;
	}
}
