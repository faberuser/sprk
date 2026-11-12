using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_PlayerCurrencyInfo
{
	public static PlayerCurrencyInfo Parse(string s)
	{
		return Parse(s, new PlayerCurrencyInfo());
	}

	public static PlayerCurrencyInfo Parse(string s, PlayerCurrencyInfo o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static PlayerCurrencyInfo Parse(IDictionary d)
	{
		return Parse(d, new PlayerCurrencyInfo());
	}

	public static PlayerCurrencyInfo Parse(IDictionary d, PlayerCurrencyInfo o)
	{
		o.CurrencyType = BaseJsonMarshaler.ParseEnum<CurrencyType>(d, "CurrencyType");
		o.Amount = BaseJsonMarshaler.ParseValueLong(d, "Amount");
		o.DailyAmount = BaseJsonMarshaler.ParseValueLong(d, "DailyAmount");
		o.DailyResetTime = BaseJsonMarshaler.ParseValueString(d, "DailyResetTime");
		o.NextResetTime = BaseJsonMarshaler.ParseValueLong(d, "NextResetTime");
		return o;
	}

	public static string EncodeJsonString(PlayerCurrencyInfo o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(PlayerCurrencyInfo o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("CurrencyType", o.CurrencyType.ToString());
		dictionary.Add("Amount", o.Amount.ToString());
		dictionary.Add("DailyAmount", o.DailyAmount.ToString());
		if (o.DailyResetTime != null)
		{
			dictionary.Add("DailyResetTime", o.DailyResetTime);
		}
		dictionary.Add("NextResetTime", o.NextResetTime.ToString());
		return dictionary;
	}
}
