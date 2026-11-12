using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_GuildSuppressGroupRankerInfo
{
	public static GuildSuppressGroupRankerInfo Parse(string s)
	{
		return Parse(s, new GuildSuppressGroupRankerInfo());
	}

	public static GuildSuppressGroupRankerInfo Parse(string s, GuildSuppressGroupRankerInfo o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static GuildSuppressGroupRankerInfo Parse(IDictionary d)
	{
		return Parse(d, new GuildSuppressGroupRankerInfo());
	}

	public static GuildSuppressGroupRankerInfo Parse(IDictionary d, GuildSuppressGroupRankerInfo o)
	{
		o.Rank = BaseJsonMarshaler.ParseValueInt(d, "Rank");
		o.GuildId = BaseJsonMarshaler.ParseValueLong(d, "GuildId");
		o.GuildLogo = BaseJsonMarshaler.ParseValueInt(d, "GuildLogo");
		o.GuildName = BaseJsonMarshaler.ParseValueString(d, "GuildName");
		o.Score = BaseJsonMarshaler.ParseValueLong(d, "Score");
		o.CountryCode = BaseJsonMarshaler.ParseValueString(d, "CountryCode");
		o.ServerGroup = BaseJsonMarshaler.ParseValueString(d, "ServerGroup");
		return o;
	}

	public static string EncodeJsonString(GuildSuppressGroupRankerInfo o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(GuildSuppressGroupRankerInfo o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("Rank", o.Rank.ToString());
		dictionary.Add("GuildId", o.GuildId.ToString());
		dictionary.Add("GuildLogo", o.GuildLogo.ToString());
		if (o.GuildName != null)
		{
			dictionary.Add("GuildName", o.GuildName);
		}
		dictionary.Add("Score", o.Score.ToString());
		if (o.CountryCode != null)
		{
			dictionary.Add("CountryCode", o.CountryCode);
		}
		if (o.ServerGroup != null)
		{
			dictionary.Add("ServerGroup", o.ServerGroup);
		}
		return dictionary;
	}
}
