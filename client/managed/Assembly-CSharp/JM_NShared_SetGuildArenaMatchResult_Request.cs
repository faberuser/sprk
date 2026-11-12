using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.SetGuildArenaMatchResult;

public static class JM_NShared_SetGuildArenaMatchResult_Request
{
	public static NShared.SetGuildArenaMatchResult.Request Parse(string s)
	{
		return Parse(s, new NShared.SetGuildArenaMatchResult.Request());
	}

	public static NShared.SetGuildArenaMatchResult.Request Parse(string s, NShared.SetGuildArenaMatchResult.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.SetGuildArenaMatchResult.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.SetGuildArenaMatchResult.Request());
	}

	public static NShared.SetGuildArenaMatchResult.Request Parse(IDictionary d, NShared.SetGuildArenaMatchResult.Request o)
	{
		o.Win = BaseJsonMarshaler.ParseValueInt(d, "Win");
		o.PlayTime = BaseJsonMarshaler.ParseValueInt(d, "PlayTime");
		o.AliveHeroIndices = BaseJsonMarshaler.ParseArrayCast(d, "AliveHeroIndices", BaseJsonMarshaler.CastToInt);
		o.BattleLogString = BaseJsonMarshaler.ParseValueString(d, "BattleLogString");
		o.CreatureInfoString = BaseJsonMarshaler.ParseValueString(d, "CreatureInfoString");
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.SetGuildArenaMatchResult.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.SetGuildArenaMatchResult.Request o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("Win", o.Win.ToString());
		dictionary.Add("PlayTime", o.PlayTime.ToString());
		dictionary.Add("AliveHeroIndices", BaseJsonMarshaler.EncodeArray(o.AliveHeroIndices));
		if (o.BattleLogString != null)
		{
			dictionary.Add("BattleLogString", o.BattleLogString);
		}
		if (o.CreatureInfoString != null)
		{
			dictionary.Add("CreatureInfoString", o.CreatureInfoString);
		}
		if (o.SessionId != null)
		{
			dictionary.Add("SessionId", o.SessionId);
		}
		return dictionary;
	}
}
