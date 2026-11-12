using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_GuildArenaSeasonScoreInfo
{
	public static GuildArenaSeasonScoreInfo Parse(string s)
	{
		return Parse(s, new GuildArenaSeasonScoreInfo());
	}

	public static GuildArenaSeasonScoreInfo Parse(string s, GuildArenaSeasonScoreInfo o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static GuildArenaSeasonScoreInfo Parse(IDictionary d)
	{
		return Parse(d, new GuildArenaSeasonScoreInfo());
	}

	public static GuildArenaSeasonScoreInfo Parse(IDictionary d, GuildArenaSeasonScoreInfo o)
	{
		o.SeasonIndex = BaseJsonMarshaler.ParseValueInt(d, "SeasonIndex");
		o.ServerGroup = BaseJsonMarshaler.ParseValueString(d, "ServerGroup");
		o.GuildId = BaseJsonMarshaler.ParseValueLong(d, "GuildId");
		o.TierIndex = BaseJsonMarshaler.ParseValueInt(d, "TierIndex");
		o.Rank = BaseJsonMarshaler.ParseValueInt(d, "Rank");
		o.SeasonScore = BaseJsonMarshaler.ParseValueInt(d, "SeasonScore");
		o.SeasonWin = BaseJsonMarshaler.ParseValueInt(d, "SeasonWin");
		o.SeasonLose = BaseJsonMarshaler.ParseValueInt(d, "SeasonLose");
		o.SeasonDraw = BaseJsonMarshaler.ParseValueInt(d, "SeasonDraw");
		return o;
	}

	public static string EncodeJsonString(GuildArenaSeasonScoreInfo o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(GuildArenaSeasonScoreInfo o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("SeasonIndex", o.SeasonIndex.ToString());
		if (o.ServerGroup != null)
		{
			dictionary.Add("ServerGroup", o.ServerGroup);
		}
		dictionary.Add("GuildId", o.GuildId.ToString());
		dictionary.Add("TierIndex", o.TierIndex.ToString());
		dictionary.Add("Rank", o.Rank.ToString());
		dictionary.Add("SeasonScore", o.SeasonScore.ToString());
		dictionary.Add("SeasonWin", o.SeasonWin.ToString());
		dictionary.Add("SeasonLose", o.SeasonLose.ToString());
		dictionary.Add("SeasonDraw", o.SeasonDraw.ToString());
		return dictionary;
	}
}
