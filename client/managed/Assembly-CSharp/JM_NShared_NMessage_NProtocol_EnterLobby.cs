using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared.NMessage.NProtocol;

public static class JM_NShared_NMessage_NProtocol_EnterLobby
{
	public static EnterLobby Parse(string s)
	{
		return Parse(s, new EnterLobby());
	}

	public static EnterLobby Parse(string s, EnterLobby o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static EnterLobby Parse(IDictionary d)
	{
		return Parse(d, new EnterLobby());
	}

	public static EnterLobby Parse(IDictionary d, EnterLobby o)
	{
		return o;
	}

	public static string EncodeJsonString(EnterLobby o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(EnterLobby o)
	{
		return new Dictionary<string, object>();
	}
}
