using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.G2G_SetGuildId;

public static class JM_NShared_G2G_SetGuildId_Request
{
	public static NShared.G2G_SetGuildId.Request Parse(string s)
	{
		return Parse(s, new NShared.G2G_SetGuildId.Request());
	}

	public static NShared.G2G_SetGuildId.Request Parse(string s, NShared.G2G_SetGuildId.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.G2G_SetGuildId.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.G2G_SetGuildId.Request());
	}

	public static NShared.G2G_SetGuildId.Request Parse(IDictionary d, NShared.G2G_SetGuildId.Request o)
	{
		o.AccountId = BaseJsonMarshaler.ParseValueLong(d, "AccountId");
		o.GuildId = BaseJsonMarshaler.ParseValueLong(d, "GuildId");
		return o;
	}

	public static string EncodeJsonString(NShared.G2G_SetGuildId.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.G2G_SetGuildId.Request o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("AccountId", o.AccountId.ToString());
		dictionary.Add("GuildId", o.GuildId.ToString());
		return dictionary;
	}
}
