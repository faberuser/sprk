using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.NWorldBoss;

public static class JM_NShared_NWorldBoss_EventWorldBossInfo
{
	public static EventWorldBossInfo Parse(string s)
	{
		return Parse(s, new EventWorldBossInfo());
	}

	public static EventWorldBossInfo Parse(string s, EventWorldBossInfo o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static EventWorldBossInfo Parse(IDictionary d)
	{
		return Parse(d, new EventWorldBossInfo());
	}

	public static EventWorldBossInfo Parse(IDictionary d, EventWorldBossInfo o)
	{
		o.Index = BaseJsonMarshaler.ParseValueInt(d, "Index");
		o.Season = BaseJsonMarshaler.ParseValueInt(d, "Season");
		o.OrderNo = BaseJsonMarshaler.ParseValueInt(d, "OrderNo");
		o.BossLevel = BaseJsonMarshaler.ParseValueInt(d, "BossLevel");
		o.MonsterHp0 = BaseJsonMarshaler.ParseValueLong(d, "MonsterHp0");
		o.MaxHp = BaseJsonMarshaler.ParseValueLong(d, "MaxHp");
		o.TotalDamage = BaseJsonMarshaler.ParseValueLong(d, "TotalDamage");
		o.BeginTime = BaseJsonMarshaler.ParseValueString(d, "BeginTime");
		o.EndTime = BaseJsonMarshaler.ParseValueString(d, "EndTime");
		o.NextOpenTime = BaseJsonMarshaler.ParseValueString(d, "NextOpenTime");
		o.RewardedTime = BaseJsonMarshaler.ParseValueString(d, "RewardedTime");
		o.CreatedTime = BaseJsonMarshaler.ParseValueString(d, "CreatedTime");
		o.BattleCompletedTime = BaseJsonMarshaler.ParseValueString(d, "BattleCompletedTime");
		o.DailyBossKillCount = BaseJsonMarshaler.ParseValueInt(d, "DailyBossKillCount");
		return o;
	}

	public static string EncodeJsonString(EventWorldBossInfo o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(EventWorldBossInfo o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("Index", o.Index.ToString());
		dictionary.Add("Season", o.Season.ToString());
		dictionary.Add("OrderNo", o.OrderNo.ToString());
		dictionary.Add("BossLevel", o.BossLevel.ToString());
		dictionary.Add("MonsterHp0", o.MonsterHp0.ToString());
		dictionary.Add("MaxHp", o.MaxHp.ToString());
		dictionary.Add("TotalDamage", o.TotalDamage.ToString());
		if (o.BeginTime != null)
		{
			dictionary.Add("BeginTime", o.BeginTime);
		}
		if (o.EndTime != null)
		{
			dictionary.Add("EndTime", o.EndTime);
		}
		if (o.NextOpenTime != null)
		{
			dictionary.Add("NextOpenTime", o.NextOpenTime);
		}
		if (o.RewardedTime != null)
		{
			dictionary.Add("RewardedTime", o.RewardedTime);
		}
		if (o.CreatedTime != null)
		{
			dictionary.Add("CreatedTime", o.CreatedTime);
		}
		if (o.BattleCompletedTime != null)
		{
			dictionary.Add("BattleCompletedTime", o.BattleCompletedTime);
		}
		dictionary.Add("DailyBossKillCount", o.DailyBossKillCount.ToString());
		return dictionary;
	}
}
