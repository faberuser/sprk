using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.GetGuildArenaSessionInfo;

public static class JM_NShared_GetGuildArenaSessionInfo_Request
{
	public static NShared.GetGuildArenaSessionInfo.Request Parse(string s)
	{
		return Parse(s, new NShared.GetGuildArenaSessionInfo.Request());
	}

	public static NShared.GetGuildArenaSessionInfo.Request Parse(string s, NShared.GetGuildArenaSessionInfo.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.GetGuildArenaSessionInfo.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.GetGuildArenaSessionInfo.Request());
	}

	public static NShared.GetGuildArenaSessionInfo.Request Parse(IDictionary d, NShared.GetGuildArenaSessionInfo.Request o)
	{
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.GetGuildArenaSessionInfo.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.GetGuildArenaSessionInfo.Request o)
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
