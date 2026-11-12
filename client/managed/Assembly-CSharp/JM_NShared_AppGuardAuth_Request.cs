using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.AppGuardAuth;

public static class JM_NShared_AppGuardAuth_Request
{
	public static NShared.AppGuardAuth.Request Parse(string s)
	{
		return Parse(s, new NShared.AppGuardAuth.Request());
	}

	public static NShared.AppGuardAuth.Request Parse(string s, NShared.AppGuardAuth.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.AppGuardAuth.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.AppGuardAuth.Request());
	}

	public static NShared.AppGuardAuth.Request Parse(IDictionary d, NShared.AppGuardAuth.Request o)
	{
		o.AppGuardAuthResult = BaseJsonMarshaler.ParseValueInt(d, "AppGuardAuthResult");
		o.AppGuardAuthId = BaseJsonMarshaler.ParseValueString(d, "AppGuardAuthId");
		o.ResultData = BaseJsonMarshaler.ParseValueString(d, "ResultData");
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.AppGuardAuth.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.AppGuardAuth.Request o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("AppGuardAuthResult", o.AppGuardAuthResult.ToString());
		if (o.AppGuardAuthId != null)
		{
			dictionary.Add("AppGuardAuthId", o.AppGuardAuthId);
		}
		if (o.ResultData != null)
		{
			dictionary.Add("ResultData", o.ResultData);
		}
		if (o.SessionId != null)
		{
			dictionary.Add("SessionId", o.SessionId);
		}
		return dictionary;
	}
}
