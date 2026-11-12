using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_GuildArenaSessionInfo
{
	public static GuildArenaSessionInfo Parse(string s)
	{
		return Parse(s, new GuildArenaSessionInfo());
	}

	public static GuildArenaSessionInfo Parse(string s, GuildArenaSessionInfo o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static GuildArenaSessionInfo Parse(IDictionary d)
	{
		return Parse(d, new GuildArenaSessionInfo());
	}

	public static GuildArenaSessionInfo Parse(IDictionary d, GuildArenaSessionInfo o)
	{
		o.SessionIndex = BaseJsonMarshaler.ParseValueInt(d, "SessionIndex");
		o.SeasonIndex = BaseJsonMarshaler.ParseValueInt(d, "SeasonIndex");
		o.SessionNumber = BaseJsonMarshaler.ParseValueInt(d, "SessionNumber");
		o.BeginTime = BaseJsonMarshaler.ParseValueString(d, "BeginTime");
		o.EndTime = BaseJsonMarshaler.ParseValueString(d, "EndTime");
		o.ApplyEndTime = BaseJsonMarshaler.ParseValueString(d, "ApplyEndTime");
		o.BattleBeginTime = BaseJsonMarshaler.ParseValueString(d, "BattleBeginTime");
		o.BattleEndTime = BaseJsonMarshaler.ParseValueString(d, "BattleEndTime");
		o.State = BaseJsonMarshaler.ParseEnum<GuildArenaSessionState>(d, "State");
		o.IsFirstSession = BaseJsonMarshaler.ParseValueInt(d, "IsFirstSession");
		o.IsLastSession = BaseJsonMarshaler.ParseValueInt(d, "IsLastSession");
		o.ApplyEndedTime = BaseJsonMarshaler.ParseValueString(d, "ApplyEndedTime");
		o.SessionRewardedTime = BaseJsonMarshaler.ParseValueString(d, "SessionRewardedTime");
		o.CompletedRewardedTime = BaseJsonMarshaler.ParseValueString(d, "CompletedRewardedTime");
		return o;
	}

	public static string EncodeJsonString(GuildArenaSessionInfo o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(GuildArenaSessionInfo o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("SessionIndex", o.SessionIndex.ToString());
		dictionary.Add("SeasonIndex", o.SeasonIndex.ToString());
		dictionary.Add("SessionNumber", o.SessionNumber.ToString());
		if (o.BeginTime != null)
		{
			dictionary.Add("BeginTime", o.BeginTime);
		}
		if (o.EndTime != null)
		{
			dictionary.Add("EndTime", o.EndTime);
		}
		if (o.ApplyEndTime != null)
		{
			dictionary.Add("ApplyEndTime", o.ApplyEndTime);
		}
		if (o.BattleBeginTime != null)
		{
			dictionary.Add("BattleBeginTime", o.BattleBeginTime);
		}
		if (o.BattleEndTime != null)
		{
			dictionary.Add("BattleEndTime", o.BattleEndTime);
		}
		dictionary.Add("State", o.State.ToString());
		dictionary.Add("IsFirstSession", o.IsFirstSession.ToString());
		dictionary.Add("IsLastSession", o.IsLastSession.ToString());
		if (o.ApplyEndedTime != null)
		{
			dictionary.Add("ApplyEndedTime", o.ApplyEndedTime);
		}
		if (o.SessionRewardedTime != null)
		{
			dictionary.Add("SessionRewardedTime", o.SessionRewardedTime);
		}
		if (o.CompletedRewardedTime != null)
		{
			dictionary.Add("CompletedRewardedTime", o.CompletedRewardedTime);
		}
		return dictionary;
	}
}
