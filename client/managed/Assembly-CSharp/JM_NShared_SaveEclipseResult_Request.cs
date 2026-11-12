using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.SaveEclipseResult;

public static class JM_NShared_SaveEclipseResult_Request
{
	public static NShared.SaveEclipseResult.Request Parse(string s)
	{
		return Parse(s, new NShared.SaveEclipseResult.Request());
	}

	public static NShared.SaveEclipseResult.Request Parse(string s, NShared.SaveEclipseResult.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.SaveEclipseResult.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.SaveEclipseResult.Request());
	}

	public static NShared.SaveEclipseResult.Request Parse(IDictionary d, NShared.SaveEclipseResult.Request o)
	{
		o.AccountId = BaseJsonMarshaler.ParseValueLong(d, "AccountId");
		o.PlayTime = BaseJsonMarshaler.ParseValueInt(d, "PlayTime");
		o.TotalDamage = BaseJsonMarshaler.ParseValueLong(d, "TotalDamage");
		o.MatchIndex = BaseJsonMarshaler.ParseValueInt(d, "MatchIndex");
		o.CurrentDeckIndex = BaseJsonMarshaler.ParseValueInt(d, "CurrentDeckIndex");
		o.CurrentChapterIndex = BaseJsonMarshaler.ParseValueInt(d, "CurrentChapterIndex");
		o.CurrentDungeonIndex = BaseJsonMarshaler.ParseValueInt(d, "CurrentDungeonIndex");
		o.CurrentWaveIndex = BaseJsonMarshaler.ParseValueInt(d, "CurrentWaveIndex");
		o.MaxWaveIndex = BaseJsonMarshaler.ParseValueInt(d, "MaxWaveIndex");
		o.CurrentCreatureLevel = BaseJsonMarshaler.ParseValueInt(d, "CurrentCreatureLevel");
		o.LastStatus = BaseJsonMarshaler.ParseEnum<EclipseStatus>(d, "LastStatus");
		o.BattleLogString = BaseJsonMarshaler.ParseValueString(d, "BattleLogString");
		o.CreatureInfoString = BaseJsonMarshaler.ParseValueString(d, "CreatureInfoString");
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.SaveEclipseResult.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.SaveEclipseResult.Request o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("AccountId", o.AccountId.ToString());
		dictionary.Add("PlayTime", o.PlayTime.ToString());
		dictionary.Add("TotalDamage", o.TotalDamage.ToString());
		dictionary.Add("MatchIndex", o.MatchIndex.ToString());
		dictionary.Add("CurrentDeckIndex", o.CurrentDeckIndex.ToString());
		dictionary.Add("CurrentChapterIndex", o.CurrentChapterIndex.ToString());
		dictionary.Add("CurrentDungeonIndex", o.CurrentDungeonIndex.ToString());
		dictionary.Add("CurrentWaveIndex", o.CurrentWaveIndex.ToString());
		dictionary.Add("MaxWaveIndex", o.MaxWaveIndex.ToString());
		dictionary.Add("CurrentCreatureLevel", o.CurrentCreatureLevel.ToString());
		dictionary.Add("LastStatus", o.LastStatus.ToString());
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
