using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared.NMessage.NProtocol;

public static class JM_NShared_NMessage_NProtocol_GuildLogout
{
	public static GuildLogout Parse(string s)
	{
		return Parse(s, new GuildLogout());
	}

	public static GuildLogout Parse(string s, GuildLogout o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static GuildLogout Parse(IDictionary d)
	{
		return Parse(d, new GuildLogout());
	}

	public static GuildLogout Parse(IDictionary d, GuildLogout o)
	{
		return o;
	}

	public static string EncodeJsonString(GuildLogout o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(GuildLogout o)
	{
		return new Dictionary<string, object>();
	}
}
