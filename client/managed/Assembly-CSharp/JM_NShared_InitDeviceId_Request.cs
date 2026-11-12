using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.InitDeviceId;

public static class JM_NShared_InitDeviceId_Request
{
	public static NShared.InitDeviceId.Request Parse(string s)
	{
		return Parse(s, new NShared.InitDeviceId.Request());
	}

	public static NShared.InitDeviceId.Request Parse(string s, NShared.InitDeviceId.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.InitDeviceId.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.InitDeviceId.Request());
	}

	public static NShared.InitDeviceId.Request Parse(IDictionary d, NShared.InitDeviceId.Request o)
	{
		o.DeviceId = BaseJsonMarshaler.ParseValueString(d, "DeviceId");
		o.CountryCode = BaseJsonMarshaler.ParseValueString(d, "CountryCode");
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.InitDeviceId.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.InitDeviceId.Request o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		if (o.DeviceId != null)
		{
			dictionary.Add("DeviceId", o.DeviceId);
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
