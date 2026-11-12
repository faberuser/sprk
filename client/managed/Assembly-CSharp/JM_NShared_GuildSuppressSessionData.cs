using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_GuildSuppressSessionData
{
	public static GuildSuppressSessionData Parse(string s)
	{
		return Parse(s, new GuildSuppressSessionData());
	}

	public static GuildSuppressSessionData Parse(string s, GuildSuppressSessionData o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static GuildSuppressSessionData Parse(IDictionary d)
	{
		return Parse(d, new GuildSuppressSessionData());
	}

	public static GuildSuppressSessionData Parse(IDictionary d, GuildSuppressSessionData o)
	{
		o.SessionIndex = BaseJsonMarshaler.ParseValueInt(d, "SessionIndex");
		o.SeasonIndex = BaseJsonMarshaler.ParseValueInt(d, "SeasonIndex");
		o.GlobalApplyStart = BaseJsonMarshaler.ParseValueString(d, "GlobalApplyStart");
		o.GlobalApplyEnd = BaseJsonMarshaler.ParseValueString(d, "GlobalApplyEnd");
		o.GlobalBattleStart = BaseJsonMarshaler.ParseValueString(d, "GlobalBattleStart");
		o.GlobalBattleEnd = BaseJsonMarshaler.ParseValueString(d, "GlobalBattleEnd");
		o.ViewRankSeason = BaseJsonMarshaler.ParseValueInt(d, "ViewRankSeason");
		o.ViewSession = BaseJsonMarshaler.ParseValueInt(d, "ViewSession");
		o.SuppressDungeonIndex = BaseJsonMarshaler.ParseArrayCast(d, "SuppressDungeonIndex", BaseJsonMarshaler.CastToInt);
		o.RaidIndex = BaseJsonMarshaler.ParseValueInt(d, "RaidIndex");
		o.Level = BaseJsonMarshaler.ParseArrayCast(d, "Level", BaseJsonMarshaler.CastToInt);
		o.SingleLevel = BaseJsonMarshaler.ParseArrayCast(d, "SingleLevel", BaseJsonMarshaler.CastToInt);
		o.PortalViewSession = BaseJsonMarshaler.ParseValueInt(d, "PortalViewSession");
		return o;
	}

	public static string EncodeJsonString(GuildSuppressSessionData o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(GuildSuppressSessionData o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("SessionIndex", o.SessionIndex.ToString());
		dictionary.Add("SeasonIndex", o.SeasonIndex.ToString());
		if (o.GlobalApplyStart != null)
		{
			dictionary.Add("GlobalApplyStart", o.GlobalApplyStart);
		}
		if (o.GlobalApplyEnd != null)
		{
			dictionary.Add("GlobalApplyEnd", o.GlobalApplyEnd);
		}
		if (o.GlobalBattleStart != null)
		{
			dictionary.Add("GlobalBattleStart", o.GlobalBattleStart);
		}
		if (o.GlobalBattleEnd != null)
		{
			dictionary.Add("GlobalBattleEnd", o.GlobalBattleEnd);
		}
		dictionary.Add("ViewRankSeason", o.ViewRankSeason.ToString());
		dictionary.Add("ViewSession", o.ViewSession.ToString());
		dictionary.Add("SuppressDungeonIndex", BaseJsonMarshaler.EncodeArray(o.SuppressDungeonIndex));
		dictionary.Add("RaidIndex", o.RaidIndex.ToString());
		dictionary.Add("Level", BaseJsonMarshaler.EncodeArray(o.Level));
		dictionary.Add("SingleLevel", BaseJsonMarshaler.EncodeArray(o.SingleLevel));
		dictionary.Add("PortalViewSession", o.PortalViewSession.ToString());
		return dictionary;
	}
}
