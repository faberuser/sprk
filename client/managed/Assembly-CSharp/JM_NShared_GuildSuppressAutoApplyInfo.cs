using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_GuildSuppressAutoApplyInfo
{
	public static GuildSuppressAutoApplyInfo Parse(string s)
	{
		return Parse(s, new GuildSuppressAutoApplyInfo());
	}

	public static GuildSuppressAutoApplyInfo Parse(string s, GuildSuppressAutoApplyInfo o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static GuildSuppressAutoApplyInfo Parse(IDictionary d)
	{
		return Parse(d, new GuildSuppressAutoApplyInfo());
	}

	public static GuildSuppressAutoApplyInfo Parse(IDictionary d, GuildSuppressAutoApplyInfo o)
	{
		o.GuildId = BaseJsonMarshaler.ParseValueLong(d, "GuildId");
		o.ServerGroup = BaseJsonMarshaler.ParseValueString(d, "ServerGroup");
		o.AutoApplySuppress = BaseJsonMarshaler.ParseValueInt(d, "AutoApplySuppress");
		return o;
	}

	public static string EncodeJsonString(GuildSuppressAutoApplyInfo o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(GuildSuppressAutoApplyInfo o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("GuildId", o.GuildId.ToString());
		if (o.ServerGroup != null)
		{
			dictionary.Add("ServerGroup", o.ServerGroup);
		}
		dictionary.Add("AutoApplySuppress", o.AutoApplySuppress.ToString());
		return dictionary;
	}
}
