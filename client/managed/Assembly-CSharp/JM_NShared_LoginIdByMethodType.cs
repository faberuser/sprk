using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_LoginIdByMethodType
{
	public static LoginIdByMethodType Parse(string s)
	{
		return Parse(s, new LoginIdByMethodType());
	}

	public static LoginIdByMethodType Parse(string s, LoginIdByMethodType o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static LoginIdByMethodType Parse(IDictionary d)
	{
		return Parse(d, new LoginIdByMethodType());
	}

	public static LoginIdByMethodType Parse(IDictionary d, LoginIdByMethodType o)
	{
		o.LoginMethod = BaseJsonMarshaler.ParseEnum<LoginMethodType>(d, "LoginMethod");
		o.LoginId = BaseJsonMarshaler.ParseValueString(d, "LoginId");
		return o;
	}

	public static string EncodeJsonString(LoginIdByMethodType o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(LoginIdByMethodType o)
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
		return dictionary;
	}
}
