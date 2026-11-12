using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_GuildRaidPlayingInfo
{
	public static GuildRaidPlayingInfo Parse(string s)
	{
		return Parse(s, new GuildRaidPlayingInfo());
	}

	public static GuildRaidPlayingInfo Parse(string s, GuildRaidPlayingInfo o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static GuildRaidPlayingInfo Parse(IDictionary d)
	{
		return Parse(d, new GuildRaidPlayingInfo());
	}

	public static GuildRaidPlayingInfo Parse(IDictionary d, GuildRaidPlayingInfo o)
	{
		o.PlayingAccountId = BaseJsonMarshaler.ParseValueLong(d, "PlayingAccountId");
		o.TouchedTime = BaseJsonMarshaler.ParseValueString(d, "TouchedTime");
		return o;
	}

	public static string EncodeJsonString(GuildRaidPlayingInfo o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(GuildRaidPlayingInfo o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("PlayingAccountId", o.PlayingAccountId.ToString());
		if (o.TouchedTime != null)
		{
			dictionary.Add("TouchedTime", o.TouchedTime);
		}
		return dictionary;
	}
}
