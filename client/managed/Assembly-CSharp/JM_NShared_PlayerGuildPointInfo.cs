using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_PlayerGuildPointInfo
{
	public static PlayerGuildPointInfo Parse(string s)
	{
		return Parse(s, new PlayerGuildPointInfo());
	}

	public static PlayerGuildPointInfo Parse(string s, PlayerGuildPointInfo o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static PlayerGuildPointInfo Parse(IDictionary d)
	{
		return Parse(d, new PlayerGuildPointInfo());
	}

	public static PlayerGuildPointInfo Parse(IDictionary d, PlayerGuildPointInfo o)
	{
		o.GuildPoint = BaseJsonMarshaler.ParseValueLong(d, "GuildPoint");
		o.DailyAccGuildPoint = BaseJsonMarshaler.ParseValueLong(d, "DailyAccGuildPoint");
		o.DailyAccGuildPointResetTime = BaseJsonMarshaler.ParseValueString(d, "DailyAccGuildPointResetTime");
		return o;
	}

	public static string EncodeJsonString(PlayerGuildPointInfo o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(PlayerGuildPointInfo o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("GuildPoint", o.GuildPoint.ToString());
		dictionary.Add("DailyAccGuildPoint", o.DailyAccGuildPoint.ToString());
		if (o.DailyAccGuildPointResetTime != null)
		{
			dictionary.Add("DailyAccGuildPointResetTime", o.DailyAccGuildPointResetTime);
		}
		return dictionary;
	}
}
