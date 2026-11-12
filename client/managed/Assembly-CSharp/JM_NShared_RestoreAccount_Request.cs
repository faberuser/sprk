using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.RestoreAccount;

public static class JM_NShared_RestoreAccount_Request
{
	public static NShared.RestoreAccount.Request Parse(string s)
	{
		return Parse(s, new NShared.RestoreAccount.Request());
	}

	public static NShared.RestoreAccount.Request Parse(string s, NShared.RestoreAccount.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.RestoreAccount.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.RestoreAccount.Request());
	}

	public static NShared.RestoreAccount.Request Parse(IDictionary d, NShared.RestoreAccount.Request o)
	{
		o.LoginMethod = BaseJsonMarshaler.ParseEnum<LoginMethodType>(d, "LoginMethod");
		o.LoginId = BaseJsonMarshaler.ParseValueString(d, "LoginId");
		o.LegacyLoginId = BaseJsonMarshaler.ParseValueString(d, "LegacyLoginId");
		o.EncryptSalt = BaseJsonMarshaler.ParseValueLong(d, "EncryptSalt");
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.RestoreAccount.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.RestoreAccount.Request o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("LoginMethod", o.LoginMethod.ToString());
		if (o.LoginId != null)
		{
			dictionary.Add("LoginId", o.LoginId);
		}
		if (o.LegacyLoginId != null)
		{
			dictionary.Add("LegacyLoginId", o.LegacyLoginId);
		}
		dictionary.Add("EncryptSalt", o.EncryptSalt.ToString());
		if (o.SessionId != null)
		{
			dictionary.Add("SessionId", o.SessionId);
		}
		return dictionary;
	}
}
