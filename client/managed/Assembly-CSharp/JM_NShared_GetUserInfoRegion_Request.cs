using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.GetUserInfoRegion;

public static class JM_NShared_GetUserInfoRegion_Request
{
	public static NShared.GetUserInfoRegion.Request Parse(string s)
	{
		return Parse(s, new NShared.GetUserInfoRegion.Request());
	}

	public static NShared.GetUserInfoRegion.Request Parse(string s, NShared.GetUserInfoRegion.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.GetUserInfoRegion.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.GetUserInfoRegion.Request());
	}

	public static NShared.GetUserInfoRegion.Request Parse(IDictionary d, NShared.GetUserInfoRegion.Request o)
	{
		o.LoginMethod = BaseJsonMarshaler.ParseEnum<LoginMethodType>(d, "LoginMethod");
		o.LoginId = BaseJsonMarshaler.ParseValueString(d, "LoginId");
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.GetUserInfoRegion.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.GetUserInfoRegion.Request o)
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
		if (o.SessionId != null)
		{
			dictionary.Add("SessionId", o.SessionId);
		}
		return dictionary;
	}
}
