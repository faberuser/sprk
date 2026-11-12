using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_GuildSuppressSeasonInfo
{
	public static GuildSuppressSeasonInfo Parse(string s)
	{
		return Parse(s, new GuildSuppressSeasonInfo());
	}

	public static GuildSuppressSeasonInfo Parse(string s, GuildSuppressSeasonInfo o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static GuildSuppressSeasonInfo Parse(IDictionary d)
	{
		return Parse(d, new GuildSuppressSeasonInfo());
	}

	public static GuildSuppressSeasonInfo Parse(IDictionary d, GuildSuppressSeasonInfo o)
	{
		o.SeasonIndex = BaseJsonMarshaler.ParseValueInt(d, "SeasonIndex");
		return o;
	}

	public static string EncodeJsonString(GuildSuppressSeasonInfo o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(GuildSuppressSeasonInfo o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("SeasonIndex", o.SeasonIndex.ToString());
		return dictionary;
	}
}
