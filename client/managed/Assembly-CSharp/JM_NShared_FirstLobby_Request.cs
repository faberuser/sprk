using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.FirstLobby;

public static class JM_NShared_FirstLobby_Request
{
	public static NShared.FirstLobby.Request Parse(string s)
	{
		return Parse(s, new NShared.FirstLobby.Request());
	}

	public static NShared.FirstLobby.Request Parse(string s, NShared.FirstLobby.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.FirstLobby.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.FirstLobby.Request());
	}

	public static NShared.FirstLobby.Request Parse(IDictionary d, NShared.FirstLobby.Request o)
	{
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.FirstLobby.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.FirstLobby.Request o)
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
