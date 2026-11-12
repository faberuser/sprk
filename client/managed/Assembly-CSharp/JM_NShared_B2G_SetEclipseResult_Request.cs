using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.B2G_SetEclipseResult;

public static class JM_NShared_B2G_SetEclipseResult_Request
{
	public static NShared.B2G_SetEclipseResult.Request Parse(string s)
	{
		return Parse(s, new NShared.B2G_SetEclipseResult.Request());
	}

	public static NShared.B2G_SetEclipseResult.Request Parse(string s, NShared.B2G_SetEclipseResult.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.B2G_SetEclipseResult.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.B2G_SetEclipseResult.Request());
	}

	public static NShared.B2G_SetEclipseResult.Request Parse(IDictionary d, NShared.B2G_SetEclipseResult.Request o)
	{
		o.AccountId = BaseJsonMarshaler.ParseValueLong(d, "AccountId");
		o.FinishedAccountIds = BaseJsonMarshaler.ParseArrayCast(d, "FinishedAccountIds", BaseJsonMarshaler.CastToLong);
		o.PlayTime = BaseJsonMarshaler.ParseValueInt(d, "PlayTime");
		o.Win = BaseJsonMarshaler.ParseValueBool(d, "Win");
		o.IsMaster = BaseJsonMarshaler.ParseValueBool(d, "IsMaster");
		o.AvgHeroLevel = BaseJsonMarshaler.ParseValueInt(d, "AvgHeroLevel");
		o.AliveHeroIndices = BaseJsonMarshaler.ParseArrayCast(d, "AliveHeroIndices", BaseJsonMarshaler.CastToInt);
		o.TotalDamage = BaseJsonMarshaler.ParseValueLong(d, "TotalDamage");
		o.MatchIndex = BaseJsonMarshaler.ParseValueInt(d, "MatchIndex");
		o.CurrentDeckIndex = BaseJsonMarshaler.ParseValueInt(d, "CurrentDeckIndex");
		o.CurrentChapterIndex = BaseJsonMarshaler.ParseValueInt(d, "CurrentChapterIndex");
		o.CurrentDungeonIndex = BaseJsonMarshaler.ParseValueInt(d, "CurrentDungeonIndex");
		o.CurrentWaveIndex = BaseJsonMarshaler.ParseValueInt(d, "CurrentWaveIndex");
		o.MaxWaveIndex = BaseJsonMarshaler.ParseValueInt(d, "MaxWaveIndex");
		o.CurrentCreatureLevel = BaseJsonMarshaler.ParseValueInt(d, "CurrentCreatureLevel");
		o.LastStatus = BaseJsonMarshaler.ParseEnum<EclipseStatus>(d, "LastStatus");
		o.HeroConditionInfos = BaseJsonMarshaler.ParseValueString(d, "HeroConditionInfos");
		o.KillMonsterInfos = BaseJsonMarshaler.ParseValueString(d, "KillMonsterInfos");
		return o;
	}

	public static string EncodeJsonString(NShared.B2G_SetEclipseResult.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.B2G_SetEclipseResult.Request o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("AccountId", o.AccountId.ToString());
		dictionary.Add("FinishedAccountIds", BaseJsonMarshaler.EncodeArray(o.FinishedAccountIds));
		dictionary.Add("PlayTime", o.PlayTime.ToString());
		dictionary.Add("Win", o.Win.ToString());
		dictionary.Add("IsMaster", o.IsMaster.ToString());
		dictionary.Add("AvgHeroLevel", o.AvgHeroLevel.ToString());
		dictionary.Add("AliveHeroIndices", BaseJsonMarshaler.EncodeArray(o.AliveHeroIndices));
		dictionary.Add("TotalDamage", o.TotalDamage.ToString());
		dictionary.Add("MatchIndex", o.MatchIndex.ToString());
		dictionary.Add("CurrentDeckIndex", o.CurrentDeckIndex.ToString());
		dictionary.Add("CurrentChapterIndex", o.CurrentChapterIndex.ToString());
		dictionary.Add("CurrentDungeonIndex", o.CurrentDungeonIndex.ToString());
		dictionary.Add("CurrentWaveIndex", o.CurrentWaveIndex.ToString());
		dictionary.Add("MaxWaveIndex", o.MaxWaveIndex.ToString());
		dictionary.Add("CurrentCreatureLevel", o.CurrentCreatureLevel.ToString());
		dictionary.Add("LastStatus", o.LastStatus.ToString());
		if (o.HeroConditionInfos != null)
		{
			dictionary.Add("HeroConditionInfos", o.HeroConditionInfos);
		}
		if (o.KillMonsterInfos != null)
		{
			dictionary.Add("KillMonsterInfos", o.KillMonsterInfos);
		}
		return dictionary;
	}
}
