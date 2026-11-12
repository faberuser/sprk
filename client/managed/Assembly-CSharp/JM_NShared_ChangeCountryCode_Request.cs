using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.ChangeCountryCode;

public static class JM_NShared_ChangeCountryCode_Request
{
	public static NShared.ChangeCountryCode.Request Parse(string s)
	{
		return Parse(s, new NShared.ChangeCountryCode.Request());
	}

	public static NShared.ChangeCountryCode.Request Parse(string s, NShared.ChangeCountryCode.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.ChangeCountryCode.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.ChangeCountryCode.Request());
	}

	public static NShared.ChangeCountryCode.Request Parse(IDictionary d, NShared.ChangeCountryCode.Request o)
	{
		o.CountryCode = BaseJsonMarshaler.ParseValueString(d, "CountryCode");
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.ChangeCountryCode.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.ChangeCountryCode.Request o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		if (o.CountryCode != null)
		{
			dictionary.Add("CountryCode", o.CountryCode);
		}
		if (o.SessionId != null)
		{
			dictionary.Add("SessionId", o.SessionId);
		}
		return dictionary;
	}
}
