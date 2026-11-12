using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.LinkAccount;

public static class JM_NShared_LinkAccount_Request
{
	public static NShared.LinkAccount.Request Parse(string s)
	{
		return Parse(s, new NShared.LinkAccount.Request());
	}

	public static NShared.LinkAccount.Request Parse(string s, NShared.LinkAccount.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.LinkAccount.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.LinkAccount.Request());
	}

	public static NShared.LinkAccount.Request Parse(IDictionary d, NShared.LinkAccount.Request o)
	{
		o.LoginMethod = BaseJsonMarshaler.ParseEnum<LoginMethodType>(d, "LoginMethod");
		o.LinkedId = BaseJsonMarshaler.ParseValueString(d, "LinkedId");
		o.ForceLink = BaseJsonMarshaler.ParseValueBool(d, "ForceLink");
		o.LoginVerificationInfo = BaseJsonMarshaler.ParseValueString(d, "LoginVerificationInfo");
		o.AccountId = BaseJsonMarshaler.ParseValueLong(d, "AccountId");
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.LinkAccount.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.LinkAccount.Request o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("LoginMethod", o.LoginMethod.ToString());
		if (o.LinkedId != null)
		{
			dictionary.Add("LinkedId", o.LinkedId);
		}
		dictionary.Add("ForceLink", o.ForceLink.ToString());
		if (o.LoginVerificationInfo != null)
		{
			dictionary.Add("LoginVerificationInfo", o.LoginVerificationInfo);
		}
		dictionary.Add("AccountId", o.AccountId.ToString());
		if (o.SessionId != null)
		{
			dictionary.Add("SessionId", o.SessionId);
		}
		return dictionary;
	}
}
