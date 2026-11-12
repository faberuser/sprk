using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.SetPushNotification;

public static class JM_NShared_SetPushNotification_Request
{
	public static NShared.SetPushNotification.Request Parse(string s)
	{
		return Parse(s, new NShared.SetPushNotification.Request());
	}

	public static NShared.SetPushNotification.Request Parse(string s, NShared.SetPushNotification.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.SetPushNotification.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.SetPushNotification.Request());
	}

	public static NShared.SetPushNotification.Request Parse(IDictionary d, NShared.SetPushNotification.Request o)
	{
		o.PushType = BaseJsonMarshaler.ParseEnum<PushNotificationType>(d, "PushType");
		o.Status = BaseJsonMarshaler.ParseValueBool(d, "Status");
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.SetPushNotification.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.SetPushNotification.Request o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("PushType", o.PushType.ToString());
		dictionary.Add("Status", o.Status.ToString());
		if (o.SessionId != null)
		{
			dictionary.Add("SessionId", o.SessionId);
		}
		return dictionary;
	}
}
