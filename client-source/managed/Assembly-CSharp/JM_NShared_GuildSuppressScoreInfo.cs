using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_GuildSuppressScoreInfo
{
	public static GuildSuppressScoreInfo Parse(string s)
	{
		return Parse(s, new GuildSuppressScoreInfo());
	}

	public static GuildSuppressScoreInfo Parse(string s, GuildSuppressScoreInfo o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static GuildSuppressScoreInfo Parse(IDictionary d)
	{
		return Parse(d, new GuildSuppressScoreInfo());
	}

	public static GuildSuppressScoreInfo Parse(IDictionary d, GuildSuppressScoreInfo o)
	{
		o.GuildId = BaseJsonMarshaler.ParseValueInt(d, "GuildId");
		o.AccountId = BaseJsonMarshaler.ParseValueLong(d, "AccountId");
		o.Rank = BaseJsonMarshaler.ParseValueInt(d, "Rank");
		o.TotalDamage = BaseJsonMarshaler.ParseValueLong(d, "TotalDamage");
		return o;
	}

	public static string EncodeJsonString(GuildSuppressScoreInfo o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(GuildSuppressScoreInfo o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("GuildId", o.GuildId.ToString());
		dictionary.Add("AccountId", o.AccountId.ToString());
		dictionary.Add("Rank", o.Rank.ToString());
		dictionary.Add("TotalDamage", o.TotalDamage.ToString());
		return dictionary;
	}
}
