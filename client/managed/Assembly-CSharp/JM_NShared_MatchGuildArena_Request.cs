using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.MatchGuildArena;

public static class JM_NShared_MatchGuildArena_Request
{
	public static NShared.MatchGuildArena.Request Parse(string s)
	{
		return Parse(s, new NShared.MatchGuildArena.Request());
	}

	public static NShared.MatchGuildArena.Request Parse(string s, NShared.MatchGuildArena.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.MatchGuildArena.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.MatchGuildArena.Request());
	}

	public static NShared.MatchGuildArena.Request Parse(IDictionary d, NShared.MatchGuildArena.Request o)
	{
		o.ArenaType = BaseJsonMarshaler.ParseEnum<MatchArenaType>(d, "ArenaType");
		o.HeroIndices = BaseJsonMarshaler.ParseArrayCast(d, "HeroIndices", BaseJsonMarshaler.CastToInt);
		o.SessionIndex = BaseJsonMarshaler.ParseValueInt(d, "SessionIndex");
		o.EnemyServerGroup = BaseJsonMarshaler.ParseValueString(d, "EnemyServerGroup");
		o.EnemyGuildId = BaseJsonMarshaler.ParseValueLong(d, "EnemyGuildId");
		o.EnemyAccountId = BaseJsonMarshaler.ParseValueLong(d, "EnemyAccountId");
		o.EnemyDeckIndex = BaseJsonMarshaler.ParseValueInt(d, "EnemyDeckIndex");
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.MatchGuildArena.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.MatchGuildArena.Request o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("ArenaType", o.ArenaType.ToString());
		dictionary.Add("HeroIndices", BaseJsonMarshaler.EncodeArray(o.HeroIndices));
		dictionary.Add("SessionIndex", o.SessionIndex.ToString());
		if (o.EnemyServerGroup != null)
		{
			dictionary.Add("EnemyServerGroup", o.EnemyServerGroup);
		}
		dictionary.Add("EnemyGuildId", o.EnemyGuildId.ToString());
		dictionary.Add("EnemyAccountId", o.EnemyAccountId.ToString());
		dictionary.Add("EnemyDeckIndex", o.EnemyDeckIndex.ToString());
		if (o.SessionId != null)
		{
			dictionary.Add("SessionId", o.SessionId);
		}
		return dictionary;
	}
}
