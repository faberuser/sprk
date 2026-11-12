using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.OrdealInfo;

public static class JM_NShared_OrdealInfo_Request
{
	public static NShared.OrdealInfo.Request Parse(string s)
	{
		return Parse(s, new NShared.OrdealInfo.Request());
	}

	public static NShared.OrdealInfo.Request Parse(string s, NShared.OrdealInfo.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.OrdealInfo.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.OrdealInfo.Request());
	}

	public static NShared.OrdealInfo.Request Parse(IDictionary d, NShared.OrdealInfo.Request o)
	{
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.OrdealInfo.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.OrdealInfo.Request o)
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
