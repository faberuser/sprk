using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.GetGuildSuppressSessionInfo;

public static class JM_NShared_GetGuildSuppressSessionInfo_Request
{
	public static NShared.GetGuildSuppressSessionInfo.Request Parse(string s)
	{
		return Parse(s, new NShared.GetGuildSuppressSessionInfo.Request());
	}

	public static NShared.GetGuildSuppressSessionInfo.Request Parse(string s, NShared.GetGuildSuppressSessionInfo.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.GetGuildSuppressSessionInfo.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.GetGuildSuppressSessionInfo.Request());
	}

	public static NShared.GetGuildSuppressSessionInfo.Request Parse(IDictionary d, NShared.GetGuildSuppressSessionInfo.Request o)
	{
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.GetGuildSuppressSessionInfo.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.GetGuildSuppressSessionInfo.Request o)
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
