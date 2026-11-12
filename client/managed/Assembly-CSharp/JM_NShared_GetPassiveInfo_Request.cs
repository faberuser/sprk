using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.GetPassiveInfo;

public static class JM_NShared_GetPassiveInfo_Request
{
	public static NShared.GetPassiveInfo.Request Parse(string s)
	{
		return Parse(s, new NShared.GetPassiveInfo.Request());
	}

	public static NShared.GetPassiveInfo.Request Parse(string s, NShared.GetPassiveInfo.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.GetPassiveInfo.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.GetPassiveInfo.Request());
	}

	public static NShared.GetPassiveInfo.Request Parse(IDictionary d, NShared.GetPassiveInfo.Request o)
	{
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.GetPassiveInfo.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.GetPassiveInfo.Request o)
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
