using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.EnterLobby;

public static class JM_NShared_EnterLobby_Request
{
	public static NShared.EnterLobby.Request Parse(string s)
	{
		return Parse(s, new NShared.EnterLobby.Request());
	}

	public static NShared.EnterLobby.Request Parse(string s, NShared.EnterLobby.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.EnterLobby.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.EnterLobby.Request());
	}

	public static NShared.EnterLobby.Request Parse(IDictionary d, NShared.EnterLobby.Request o)
	{
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.EnterLobby.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.EnterLobby.Request o)
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
