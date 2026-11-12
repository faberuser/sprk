using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_GuildSuppressSessionInfo
{
	public static GuildSuppressSessionInfo Parse(string s)
	{
		return Parse(s, new GuildSuppressSessionInfo());
	}

	public static GuildSuppressSessionInfo Parse(string s, GuildSuppressSessionInfo o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static GuildSuppressSessionInfo Parse(IDictionary d)
	{
		return Parse(d, new GuildSuppressSessionInfo());
	}

	public static GuildSuppressSessionInfo Parse(IDictionary d, GuildSuppressSessionInfo o)
	{
		o.SessionIndex = BaseJsonMarshaler.ParseValueLong(d, "SessionIndex");
		o.ApplyEndTime = BaseJsonMarshaler.ParseValueString(d, "ApplyEndTime");
		o.BattleEndTime = BaseJsonMarshaler.ParseValueString(d, "BattleEndTime");
		o.SessionStartedTime = BaseJsonMarshaler.ParseValueString(d, "SessionStartedTime");
		o.BattleStartedTime = BaseJsonMarshaler.ParseValueString(d, "BattleStartedTime");
		o.State = BaseJsonMarshaler.ParseEnum<GuildSuppressSessionState>(d, "State");
		return o;
	}

	public static string EncodeJsonString(GuildSuppressSessionInfo o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(GuildSuppressSessionInfo o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("SessionIndex", o.SessionIndex.ToString());
		if (o.ApplyEndTime != null)
		{
			dictionary.Add("ApplyEndTime", o.ApplyEndTime);
		}
		if (o.BattleEndTime != null)
		{
			dictionary.Add("BattleEndTime", o.BattleEndTime);
		}
		if (o.SessionStartedTime != null)
		{
			dictionary.Add("SessionStartedTime", o.SessionStartedTime);
		}
		if (o.BattleStartedTime != null)
		{
			dictionary.Add("BattleStartedTime", o.BattleStartedTime);
		}
		dictionary.Add("State", o.State.ToString());
		return dictionary;
	}
}
