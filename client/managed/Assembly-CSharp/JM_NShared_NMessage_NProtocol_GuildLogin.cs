using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared.NMessage.NProtocol;

public static class JM_NShared_NMessage_NProtocol_GuildLogin
{
	public static GuildLogin Parse(string s)
	{
		return Parse(s, new GuildLogin());
	}

	public static GuildLogin Parse(string s, GuildLogin o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static GuildLogin Parse(IDictionary d)
	{
		return Parse(d, new GuildLogin());
	}

	public static GuildLogin Parse(IDictionary d, GuildLogin o)
	{
		return o;
	}

	public static string EncodeJsonString(GuildLogin o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(GuildLogin o)
	{
		return new Dictionary<string, object>();
	}
}
