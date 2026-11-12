using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.AppGuardViolation;

public static class JM_NShared_AppGuardViolation_Request
{
	public static NShared.AppGuardViolation.Request Parse(string s)
	{
		return Parse(s, new NShared.AppGuardViolation.Request());
	}

	public static NShared.AppGuardViolation.Request Parse(string s, NShared.AppGuardViolation.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.AppGuardViolation.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.AppGuardViolation.Request());
	}

	public static NShared.AppGuardViolation.Request Parse(IDictionary d, NShared.AppGuardViolation.Request o)
	{
		o.IsEmulateDetected = BaseJsonMarshaler.ParseValueBool(d, "IsEmulateDetected");
		o.ResultData = BaseJsonMarshaler.ParseValueString(d, "ResultData");
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.AppGuardViolation.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.AppGuardViolation.Request o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("IsEmulateDetected", o.IsEmulateDetected.ToString());
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
