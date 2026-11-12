using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_GuildArenaAutoApplyInfo
{
	public static GuildArenaAutoApplyInfo Parse(string s)
	{
		return Parse(s, new GuildArenaAutoApplyInfo());
	}

	public static GuildArenaAutoApplyInfo Parse(string s, GuildArenaAutoApplyInfo o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static GuildArenaAutoApplyInfo Parse(IDictionary d)
	{
		return Parse(d, new GuildArenaAutoApplyInfo());
	}

	public static GuildArenaAutoApplyInfo Parse(IDictionary d, GuildArenaAutoApplyInfo o)
	{
		o.ServerGroup = BaseJsonMarshaler.ParseValueString(d, "ServerGroup");
		o.GuildId = BaseJsonMarshaler.ParseValueLong(d, "GuildId");
		o.AutoApply = BaseJsonMarshaler.ParseValueInt(d, "AutoApply");
		return o;
	}

	public static string EncodeJsonString(GuildArenaAutoApplyInfo o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(GuildArenaAutoApplyInfo o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		if (o.ServerGroup != null)
		{
			dictionary.Add("ServerGroup", o.ServerGroup);
		}
		dictionary.Add("GuildId", o.GuildId.ToString());
		dictionary.Add("AutoApply", o.AutoApply.ToString());
		return dictionary;
	}
}
