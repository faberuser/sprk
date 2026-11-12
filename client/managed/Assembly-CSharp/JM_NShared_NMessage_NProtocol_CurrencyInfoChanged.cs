using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.NMessage.NProtocol;

public static class JM_NShared_NMessage_NProtocol_CurrencyInfoChanged
{
	public static CurrencyInfoChanged Parse(string s)
	{
		return Parse(s, new CurrencyInfoChanged());
	}

	public static CurrencyInfoChanged Parse(string s, CurrencyInfoChanged o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static CurrencyInfoChanged Parse(IDictionary d)
	{
		return Parse(d, new CurrencyInfoChanged());
	}

	public static CurrencyInfoChanged Parse(IDictionary d, CurrencyInfoChanged o)
	{
		o.CurrencyType = BaseJsonMarshaler.ParseEnum<CurrencyType>(d, "CurrencyType");
		o.Amount = BaseJsonMarshaler.ParseValueLong(d, "Amount");
		return o;
	}

	public static string EncodeJsonString(CurrencyInfoChanged o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(CurrencyInfoChanged o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("CurrencyType", o.CurrencyType.ToString());
		dictionary.Add("Amount", o.Amount.ToString());
		return dictionary;
	}
}
