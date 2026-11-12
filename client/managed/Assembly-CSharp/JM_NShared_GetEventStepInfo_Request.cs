using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.GetEventStepInfo;

public static class JM_NShared_GetEventStepInfo_Request
{
	public static NShared.GetEventStepInfo.Request Parse(string s)
	{
		return Parse(s, new NShared.GetEventStepInfo.Request());
	}

	public static NShared.GetEventStepInfo.Request Parse(string s, NShared.GetEventStepInfo.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.GetEventStepInfo.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.GetEventStepInfo.Request());
	}

	public static NShared.GetEventStepInfo.Request Parse(IDictionary d, NShared.GetEventStepInfo.Request o)
	{
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.GetEventStepInfo.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.GetEventStepInfo.Request o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		if (o.SessionId != null)
		{
			dictionary.Add("SessionId", o.SessionId);
		}
		return dictionary;
	}
}
