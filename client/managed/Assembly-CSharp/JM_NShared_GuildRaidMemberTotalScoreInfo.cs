using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_GuildRaidMemberTotalScoreInfo
{
	public static GuildRaidMemberTotalScoreInfo Parse(string s)
	{
		return Parse(s, new GuildRaidMemberTotalScoreInfo());
	}

	public static GuildRaidMemberTotalScoreInfo Parse(string s, GuildRaidMemberTotalScoreInfo o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static GuildRaidMemberTotalScoreInfo Parse(IDictionary d)
	{
		return Parse(d, new GuildRaidMemberTotalScoreInfo());
	}

	public static GuildRaidMemberTotalScoreInfo Parse(IDictionary d, GuildRaidMemberTotalScoreInfo o)
	{
		o.AccountId = BaseJsonMarshaler.ParseValueLong(d, "AccountId");
		o.GuildId = BaseJsonMarshaler.ParseValueInt(d, "GuildId");
		o.Score = BaseJsonMarshaler.ParseValueLong(d, "Score");
		return o;
	}

	public static string EncodeJsonString(GuildRaidMemberTotalScoreInfo o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(GuildRaidMemberTotalScoreInfo o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("AccountId", o.AccountId.ToString());
		dictionary.Add("GuildId", o.GuildId.ToString());
		dictionary.Add("Score", o.Score.ToString());
		return dictionary;
	}
}
