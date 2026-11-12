using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.SetOfflineMatchResult;

public static class JM_NShared_SetOfflineMatchResult_Request
{
	public static NShared.SetOfflineMatchResult.Request Parse(string s)
	{
		return Parse(s, new NShared.SetOfflineMatchResult.Request());
	}

	public static NShared.SetOfflineMatchResult.Request Parse(string s, NShared.SetOfflineMatchResult.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.SetOfflineMatchResult.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.SetOfflineMatchResult.Request());
	}

	public static NShared.SetOfflineMatchResult.Request Parse(IDictionary d, NShared.SetOfflineMatchResult.Request o)
	{
		o.Win = BaseJsonMarshaler.ParseValueInt(d, "Win");
		o.PlayTime = BaseJsonMarshaler.ParseValueInt(d, "PlayTime");
		o.AliveHeroIndices = BaseJsonMarshaler.ParseArrayCast(d, "AliveHeroIndices", BaseJsonMarshaler.CastToInt);
		o.BattleLogString = BaseJsonMarshaler.ParseValueString(d, "BattleLogString");
		o.CreatureInfoString = BaseJsonMarshaler.ParseValueString(d, "CreatureInfoString");
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.SetOfflineMatchResult.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.SetOfflineMatchResult.Request o)
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
