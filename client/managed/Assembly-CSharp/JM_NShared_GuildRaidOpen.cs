using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_GuildRaidOpen
{
	public static GuildRaidOpen Parse(string s)
	{
		return Parse(s, new GuildRaidOpen());
	}

	public static GuildRaidOpen Parse(string s, GuildRaidOpen o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static GuildRaidOpen Parse(IDictionary d)
	{
		return Parse(d, new GuildRaidOpen());
	}

	public static GuildRaidOpen Parse(IDictionary d, GuildRaidOpen o)
	{
		o.GuildId = BaseJsonMarshaler.ParseValueInt(d, "GuildId");
		o.Status = BaseJsonMarshaler.ParseEnum<EGuidRaidOpenStatus>(d, "Status");
		o.OpenedTime = BaseJsonMarshaler.ParseValueString(d, "OpenedTime");
		return o;
	}

	public static string EncodeJsonString(GuildRaidOpen o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(GuildRaidOpen o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("GuildId", o.GuildId.ToString());
		dictionary.Add("Status", o.Status.ToString());
		if (o.OpenedTime != null)
		{
			dictionary.Add("OpenedTime", o.OpenedTime);
		}
		return dictionary;
	}
}
