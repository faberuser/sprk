using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_GuildArenaSessionScoreInfo
{
	public static GuildArenaSessionScoreInfo Parse(string s)
	{
		return Parse(s, new GuildArenaSessionScoreInfo());
	}

	public static GuildArenaSessionScoreInfo Parse(string s, GuildArenaSessionScoreInfo o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static GuildArenaSessionScoreInfo Parse(IDictionary d)
	{
		return Parse(d, new GuildArenaSessionScoreInfo());
	}

	public static GuildArenaSessionScoreInfo Parse(IDictionary d, GuildArenaSessionScoreInfo o)
	{
		o.SessionIndex = BaseJsonMarshaler.ParseValueInt(d, "SessionIndex");
		o.SeasonIndex = BaseJsonMarshaler.ParseValueInt(d, "SeasonIndex");
		o.ServerGroup = BaseJsonMarshaler.ParseValueString(d, "ServerGroup");
		o.GuildId = BaseJsonMarshaler.ParseValueLong(d, "GuildId");
		o.EnemyServerGroup = BaseJsonMarshaler.ParseValueString(d, "EnemyServerGroup");
		o.EnemyGuildId = BaseJsonMarshaler.ParseValueLong(d, "EnemyGuildId");
		o.SessionScore = BaseJsonMarshaler.ParseValueInt(d, "SessionScore");
		o.AddSeasonScore = BaseJsonMarshaler.ParseValueInt(d, "AddSeasonScore");
		o.WinResult = BaseJsonMarshaler.ParseValueInt(d, "WinResult");
		o.MVPMemberId = BaseJsonMarshaler.ParseValueLong(d, "MVPMemberId");
		o.MVPSessionScore = BaseJsonMarshaler.ParseValueInt(d, "MVPSessionScore");
		return o;
	}

	public static string EncodeJsonString(GuildArenaSessionScoreInfo o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(GuildArenaSessionScoreInfo o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("SessionIndex", o.SessionIndex.ToString());
		dictionary.Add("SeasonIndex", o.SeasonIndex.ToString());
		if (o.ServerGroup != null)
		{
			dictionary.Add("ServerGroup", o.ServerGroup);
		}
		dictionary.Add("GuildId", o.GuildId.ToString());
		if (o.EnemyServerGroup != null)
		{
			dictionary.Add("EnemyServerGroup", o.EnemyServerGroup);
		}
		dictionary.Add("EnemyGuildId", o.EnemyGuildId.ToString());
		dictionary.Add("SessionScore", o.SessionScore.ToString());
		dictionary.Add("AddSeasonScore", o.AddSeasonScore.ToString());
		dictionary.Add("WinResult", o.WinResult.ToString());
		dictionary.Add("MVPMemberId", o.MVPMemberId.ToString());
		dictionary.Add("MVPSessionScore", o.MVPSessionScore.ToString());
		return dictionary;
	}
}
