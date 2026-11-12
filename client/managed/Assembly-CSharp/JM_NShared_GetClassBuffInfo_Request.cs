using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.GetClassBuffInfo;

public static class JM_NShared_GetClassBuffInfo_Request
{
	public static NShared.GetClassBuffInfo.Request Parse(string s)
	{
		return Parse(s, new NShared.GetClassBuffInfo.Request());
	}

	public static NShared.GetClassBuffInfo.Request Parse(string s, NShared.GetClassBuffInfo.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.GetClassBuffInfo.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.GetClassBuffInfo.Request());
	}

	public static NShared.GetClassBuffInfo.Request Parse(IDictionary d, NShared.GetClassBuffInfo.Request o)
	{
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.GetClassBuffInfo.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.GetClassBuffInfo.Request o)
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
