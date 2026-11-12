using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.RequestJoinGuild;

public static class JM_NShared_RequestJoinGuild_Request
{
	public static NShared.RequestJoinGuild.Request Parse(string s)
	{
		return Parse(s, new NShared.RequestJoinGuild.Request());
	}

	public static NShared.RequestJoinGuild.Request Parse(string s, NShared.RequestJoinGuild.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.RequestJoinGuild.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.RequestJoinGuild.Request());
	}

	public static NShared.RequestJoinGuild.Request Parse(IDictionary d, NShared.RequestJoinGuild.Request o)
	{
		o.GuildId = BaseJsonMarshaler.ParseValueLong(d, "GuildId");
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.RequestJoinGuild.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.RequestJoinGuild.Request o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("GuildId", o.GuildId.ToString());
		if (o.SessionId != null)
		{
			dictionary.Add("SessionId", o.SessionId);
		}
		return dictionary;
	}
}
