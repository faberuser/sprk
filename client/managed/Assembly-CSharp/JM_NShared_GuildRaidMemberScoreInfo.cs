using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_GuildRaidMemberScoreInfo
{
	public static GuildRaidMemberScoreInfo Parse(string s)
	{
		return Parse(s, new GuildRaidMemberScoreInfo());
	}

	public static GuildRaidMemberScoreInfo Parse(string s, GuildRaidMemberScoreInfo o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static GuildRaidMemberScoreInfo Parse(IDictionary d)
	{
		return Parse(d, new GuildRaidMemberScoreInfo());
	}

	public static GuildRaidMemberScoreInfo Parse(IDictionary d, GuildRaidMemberScoreInfo o)
	{
		o.AccountId = BaseJsonMarshaler.ParseValueLong(d, "AccountId");
		o.Score = BaseJsonMarshaler.ParseValueLong(d, "Score");
		return o;
	}

	public static string EncodeJsonString(GuildRaidMemberScoreInfo o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(GuildRaidMemberScoreInfo o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("AccountId", o.AccountId.ToString());
		dictionary.Add("Score", o.Score.ToString());
		return dictionary;
	}
}
