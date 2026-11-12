using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.G2R_ChangeCountryCodeRanker;

public static class JM_NShared_G2R_ChangeCountryCodeRanker_Request
{
	public static NShared.G2R_ChangeCountryCodeRanker.Request Parse(string s)
	{
		return Parse(s, new NShared.G2R_ChangeCountryCodeRanker.Request());
	}

	public static NShared.G2R_ChangeCountryCodeRanker.Request Parse(string s, NShared.G2R_ChangeCountryCodeRanker.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.G2R_ChangeCountryCodeRanker.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.G2R_ChangeCountryCodeRanker.Request());
	}

	public static NShared.G2R_ChangeCountryCodeRanker.Request Parse(IDictionary d, NShared.G2R_ChangeCountryCodeRanker.Request o)
	{
		o.Keyword = BaseJsonMarshaler.ParseValueString(d, "Keyword");
		o.ServerName = BaseJsonMarshaler.ParseValueString(d, "ServerName");
		o.AccountId = BaseJsonMarshaler.ParseValueLong(d, "AccountId");
		return o;
	}

	public static string EncodeJsonString(NShared.G2R_ChangeCountryCodeRanker.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.G2R_ChangeCountryCodeRanker.Request o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		if (o.Keyword != null)
		{
			dictionary.Add("Keyword", o.Keyword);
		}
		if (o.ServerName != null)
		{
			dictionary.Add("ServerName", o.ServerName);
		}
		dictionary.Add("AccountId", o.AccountId.ToString());
		return dictionary;
	}
}
