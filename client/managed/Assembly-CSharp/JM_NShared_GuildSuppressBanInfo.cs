using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_GuildSuppressBanInfo
{
	public static GuildSuppressBanInfo Parse(string s)
	{
		return Parse(s, new GuildSuppressBanInfo());
	}

	public static GuildSuppressBanInfo Parse(string s, GuildSuppressBanInfo o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static GuildSuppressBanInfo Parse(IDictionary d)
	{
		return Parse(d, new GuildSuppressBanInfo());
	}

	public static GuildSuppressBanInfo Parse(IDictionary d, GuildSuppressBanInfo o)
	{
		o.SeasonIndex = BaseJsonMarshaler.ParseValueInt(d, "SeasonIndex");
		o.SessionIndex = BaseJsonMarshaler.ParseValueInt(d, "SessionIndex");
		return o;
	}

	public static string EncodeJsonString(GuildSuppressBanInfo o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(GuildSuppressBanInfo o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("SeasonIndex", o.SeasonIndex.ToString());
		dictionary.Add("SessionIndex", o.SessionIndex.ToString());
		return dictionary;
	}
}
