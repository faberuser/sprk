using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_TeamLevelData
{
	public static TeamLevelData Parse(string s)
	{
		return Parse(s, new TeamLevelData());
	}

	public static TeamLevelData Parse(string s, TeamLevelData o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static TeamLevelData Parse(IDictionary d)
	{
		return Parse(d, new TeamLevelData());
	}

	public static TeamLevelData Parse(IDictionary d, TeamLevelData o)
	{
		o.Level = BaseJsonMarshaler.ParseValueInt(d, "Level");
		o.LocalExp = BaseJsonMarshaler.ParseValueLong(d, "LocalExp");
		o.MaxStamina = BaseJsonMarshaler.ParseValueInt(d, "MaxStamina");
		o.RechargeStaminaSec = BaseJsonMarshaler.ParseValueInt(d, "RechargeStaminaSec");
		o.DailyBonusStamina = BaseJsonMarshaler.ParseValueInt(d, "DailyBonusStamina");
		o.ExpBonusPercentage = BaseJsonMarshaler.ParseValueInt(d, "ExpBonusPercentage");
		o.GoldBonusPercentage = BaseJsonMarshaler.ParseValueInt(d, "GoldBonusPercentage");
		o.DailyGold = BaseJsonMarshaler.ParseValueInt(d, "DailyGold");
		o.TeamLevelBuffOptionIndex = BaseJsonMarshaler.ParseValueInt(d, "TeamLevelBuffOptionIndex");
		o.TeamLevelBuffBonusOptionIndex = BaseJsonMarshaler.ParseValueInt(d, "TeamLevelBuffBonusOptionIndex");
		return o;
	}

	public static string EncodeJsonString(TeamLevelData o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(TeamLevelData o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("Level", o.Level.ToString());
		dictionary.Add("LocalExp", o.LocalExp.ToString());
		dictionary.Add("MaxStamina", o.MaxStamina.ToString());
		dictionary.Add("RechargeStaminaSec", o.RechargeStaminaSec.ToString());
		dictionary.Add("DailyBonusStamina", o.DailyBonusStamina.ToString());
		dictionary.Add("ExpBonusPercentage", o.ExpBonusPercentage.ToString());
		dictionary.Add("GoldBonusPercentage", o.GoldBonusPercentage.ToString());
		dictionary.Add("DailyGold", o.DailyGold.ToString());
		dictionary.Add("TeamLevelBuffOptionIndex", o.TeamLevelBuffOptionIndex.ToString());
		dictionary.Add("TeamLevelBuffBonusOptionIndex", o.TeamLevelBuffBonusOptionIndex.ToString());
		return dictionary;
	}
}
