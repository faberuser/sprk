using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.GetPlayerCurrencyInfo;

public static class JM_NShared_GetPlayerCurrencyInfo_Request
{
	public static NShared.GetPlayerCurrencyInfo.Request Parse(string s)
	{
		return Parse(s, new NShared.GetPlayerCurrencyInfo.Request());
	}

	public static NShared.GetPlayerCurrencyInfo.Request Parse(string s, NShared.GetPlayerCurrencyInfo.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.GetPlayerCurrencyInfo.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.GetPlayerCurrencyInfo.Request());
	}

	public static NShared.GetPlayerCurrencyInfo.Request Parse(IDictionary d, NShared.GetPlayerCurrencyInfo.Request o)
	{
		o.CurrencyType = BaseJsonMarshaler.ParseEnum<CurrencyType>(d, "CurrencyType");
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.GetPlayerCurrencyInfo.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.GetPlayerCurrencyInfo.Request o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("CurrencyType", o.CurrencyType.ToString());
		if (o.SessionId != null)
		{
			dictionary.Add("SessionId", o.SessionId);
		}
		return dictionary;
	}
}
