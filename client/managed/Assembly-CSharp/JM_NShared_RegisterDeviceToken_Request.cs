using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.RegisterDeviceToken;

public static class JM_NShared_RegisterDeviceToken_Request
{
	public static NShared.RegisterDeviceToken.Request Parse(string s)
	{
		return Parse(s, new NShared.RegisterDeviceToken.Request());
	}

	public static NShared.RegisterDeviceToken.Request Parse(string s, NShared.RegisterDeviceToken.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.RegisterDeviceToken.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.RegisterDeviceToken.Request());
	}

	public static NShared.RegisterDeviceToken.Request Parse(IDictionary d, NShared.RegisterDeviceToken.Request o)
	{
		o.DeviceToken = BaseJsonMarshaler.ParseValueString(d, "DeviceToken");
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.RegisterDeviceToken.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.RegisterDeviceToken.Request o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		if (o.DeviceToken != null)
		{
			dictionary.Add("DeviceToken", o.DeviceToken);
		}
		if (o.SessionId != null)
		{
			dictionary.Add("SessionId", o.SessionId);
		}
		return dictionary;
	}
}
