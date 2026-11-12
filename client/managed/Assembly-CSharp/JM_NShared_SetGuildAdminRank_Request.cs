using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.SetGuildAdminRank;

public static class JM_NShared_SetGuildAdminRank_Request
{
	public static NShared.SetGuildAdminRank.Request Parse(string s)
	{
		return Parse(s, new NShared.SetGuildAdminRank.Request());
	}

	public static NShared.SetGuildAdminRank.Request Parse(string s, NShared.SetGuildAdminRank.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.SetGuildAdminRank.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.SetGuildAdminRank.Request());
	}

	public static NShared.SetGuildAdminRank.Request Parse(IDictionary d, NShared.SetGuildAdminRank.Request o)
	{
		o.AccountId = BaseJsonMarshaler.ParseValueLong(d, "AccountId");
		o.GuildRank = BaseJsonMarshaler.ParseValueInt(d, "GuildRank");
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.SetGuildAdminRank.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.SetGuildAdminRank.Request o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("AccountId", o.AccountId.ToString());
		dictionary.Add("GuildRank", o.GuildRank.ToString());
		if (o.SessionId != null)
		{
			dictionary.Add("SessionId", o.SessionId);
		}
		return dictionary;
	}
}
