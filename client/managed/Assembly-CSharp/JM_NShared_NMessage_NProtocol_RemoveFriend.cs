using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared.NMessage.NProtocol;

public static class JM_NShared_NMessage_NProtocol_RemoveFriend
{
	public static RemoveFriend Parse(string s)
	{
		return Parse(s, new RemoveFriend());
	}

	public static RemoveFriend Parse(string s, RemoveFriend o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static RemoveFriend Parse(IDictionary d)
	{
		return Parse(d, new RemoveFriend());
	}

	public static RemoveFriend Parse(IDictionary d, RemoveFriend o)
	{
		return o;
	}

	public static string EncodeJsonString(RemoveFriend o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(RemoveFriend o)
	{
		return new Dictionary<string, object>();
	}
}
