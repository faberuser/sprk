using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.NContentsDefine;

public static class JM_NShared_NContentsDefine_CurrencyMaxCap
{
	public static CurrencyMaxCap Parse(string s)
	{
		return Parse(s, new CurrencyMaxCap());
	}

	public static CurrencyMaxCap Parse(string s, CurrencyMaxCap o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static CurrencyMaxCap Parse(IDictionary d)
	{
		return Parse(d, new CurrencyMaxCap());
	}

	public static CurrencyMaxCap Parse(IDictionary d, CurrencyMaxCap o)
	{
		o.MaxCap = BaseJsonMarshaler.ParseDictionary<CurrencyType, long>(d, "MaxCap");
		return o;
	}

	public static string EncodeJsonString(CurrencyMaxCap o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(CurrencyMaxCap o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("MaxCap", BaseJsonMarshaler.EncodeDictionary(o.MaxCap));
		return dictionary;
	}
}
