using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.GetJoinRequestPlayers;

public static class JM_NShared_GetJoinRequestPlayers_Request
{
	public static NShared.GetJoinRequestPlayers.Request Parse(string s)
	{
		return Parse(s, new NShared.GetJoinRequestPlayers.Request());
	}

	public static NShared.GetJoinRequestPlayers.Request Parse(string s, NShared.GetJoinRequestPlayers.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.GetJoinRequestPlayers.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.GetJoinRequestPlayers.Request());
	}

	public static NShared.GetJoinRequestPlayers.Request Parse(IDictionary d, NShared.GetJoinRequestPlayers.Request o)
	{
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.GetJoinRequestPlayers.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.GetJoinRequestPlayers.Request o)
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
