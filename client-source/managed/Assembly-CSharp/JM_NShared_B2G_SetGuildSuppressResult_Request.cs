using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.B2G_SetGuildSuppressResult;

public static class JM_NShared_B2G_SetGuildSuppressResult_Request
{
	public static NShared.B2G_SetGuildSuppressResult.Request Parse(string s)
	{
		return Parse(s, new NShared.B2G_SetGuildSuppressResult.Request());
	}

	public static NShared.B2G_SetGuildSuppressResult.Request Parse(string s, NShared.B2G_SetGuildSuppressResult.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.B2G_SetGuildSuppressResult.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.B2G_SetGuildSuppressResult.Request());
	}

	public static NShared.B2G_SetGuildSuppressResult.Request Parse(IDictionary d, NShared.B2G_SetGuildSuppressResult.Request o)
	{
		o.AccountId = BaseJsonMarshaler.ParseValueLong(d, "AccountId");
		o.ChapterIndex = BaseJsonMarshaler.ParseValueInt(d, "ChapterIndex");
		o.DungeonIndex = BaseJsonMarshaler.ParseValueInt(d, "DungeonIndex");
		o.RaidIndex = BaseJsonMarshaler.ParseValueInt(d, "RaidIndex");
		o.RaidLevel = BaseJsonMarshaler.ParseValueInt(d, "RaidLevel");
		o.PlayTime = BaseJsonMarshaler.ParseValueInt(d, "PlayTime");
		o.Win = BaseJsonMarshaler.ParseValueBool(d, "Win");
		o.IsMaster = BaseJsonMarshaler.ParseValueBool(d, "IsMaster");
		o.IsLogOn = BaseJsonMarshaler.ParseValueBool(d, "IsLogOn");
		o.AvgHeroLevel = BaseJsonMarshaler.ParseValueInt(d, "AvgHeroLevel");
		o.AliveHeroIndices = BaseJsonMarshaler.ParseArrayCast(d, "AliveHeroIndices", BaseJsonMarshaler.CastToInt);
		o.PartyInfos = BaseJsonMarshaler.ParseValueString(d, "PartyInfos");
		o.BossTotalDamaged = BaseJsonMarshaler.ParseValueLong(d, "BossTotalDamaged");
		o.TotalHeroCount = BaseJsonMarshaler.ParseValueInt(d, "TotalHeroCount");
		o.MaxHp = BaseJsonMarshaler.ParseValueLong(d, "MaxHp");
		o.CurrentHp = BaseJsonMarshaler.ParseValueLong(d, "CurrentHp");
		o.BattleLogString = BaseJsonMarshaler.ParseValueString(d, "BattleLogString");
		o.OfflineAccountIds = BaseJsonMarshaler.ParseArrayCast(d, "OfflineAccountIds", BaseJsonMarshaler.CastToLong);
		return o;
	}

	public static string EncodeJsonString(NShared.B2G_SetGuildSuppressResult.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.B2G_SetGuildSuppressResult.Request o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("AccountId", o.AccountId.ToString());
		dictionary.Add("ChapterIndex", o.ChapterIndex.ToString());
		dictionary.Add("DungeonIndex", o.DungeonIndex.ToString());
		dictionary.Add("RaidIndex", o.RaidIndex.ToString());
		dictionary.Add("RaidLevel", o.RaidLevel.ToString());
		dictionary.Add("PlayTime", o.PlayTime.ToString());
		dictionary.Add("Win", o.Win.ToString());
		dictionary.Add("IsMaster", o.IsMaster.ToString());
		dictionary.Add("IsLogOn", o.IsLogOn.ToString());
		dictionary.Add("AvgHeroLevel", o.AvgHeroLevel.ToString());
		dictionary.Add("AliveHeroIndices", BaseJsonMarshaler.EncodeArray(o.AliveHeroIndices));
		if (o.PartyInfos != null)
		{
			dictionary.Add("PartyInfos", o.PartyInfos);
		}
		dictionary.Add("BossTotalDamaged", o.BossTotalDamaged.ToString());
		dictionary.Add("TotalHeroCount", o.TotalHeroCount.ToString());
		dictionary.Add("MaxHp", o.MaxHp.ToString());
		dictionary.Add("CurrentHp", o.CurrentHp.ToString());
		if (o.BattleLogString != null)
		{
			dictionary.Add("BattleLogString", o.BattleLogString);
		}
		dictionary.Add("OfflineAccountIds", BaseJsonMarshaler.EncodeArray(o.OfflineAccountIds));
		return dictionary;
	}
}
