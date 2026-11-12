using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.GetMazeTowerInfo;

public static class JM_NShared_GetMazeTowerInfo_Request
{
	public static NShared.GetMazeTowerInfo.Request Parse(string s)
	{
		return Parse(s, new NShared.GetMazeTowerInfo.Request());
	}

	public static NShared.GetMazeTowerInfo.Request Parse(string s, NShared.GetMazeTowerInfo.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.GetMazeTowerInfo.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.GetMazeTowerInfo.Request());
	}

	public static NShared.GetMazeTowerInfo.Request Parse(IDictionary d, NShared.GetMazeTowerInfo.Request o)
	{
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.GetMazeTowerInfo.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.GetMazeTowerInfo.Request o)
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
