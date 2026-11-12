using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_MailCurrencyInfo
{
	public static MailCurrencyInfo Parse(string s)
	{
		return Parse(s, new MailCurrencyInfo());
	}

	public static MailCurrencyInfo Parse(string s, MailCurrencyInfo o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static MailCurrencyInfo Parse(IDictionary d)
	{
		return Parse(d, new MailCurrencyInfo());
	}

	public static MailCurrencyInfo Parse(IDictionary d, MailCurrencyInfo o)
	{
		o.CurrencyType = BaseJsonMarshaler.ParseEnum<CurrencyType>(d, "CurrencyType");
		o.Amount = BaseJsonMarshaler.ParseValueInt(d, "Amount");
		return o;
	}

	public static string EncodeJsonString(MailCurrencyInfo o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(MailCurrencyInfo o)
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
