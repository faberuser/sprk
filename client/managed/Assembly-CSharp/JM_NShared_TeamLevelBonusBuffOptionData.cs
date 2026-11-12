using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_TeamLevelBonusBuffOptionData
{
	public static TeamLevelBonusBuffOptionData Parse(string s)
	{
		return Parse(s, new TeamLevelBonusBuffOptionData());
	}

	public static TeamLevelBonusBuffOptionData Parse(string s, TeamLevelBonusBuffOptionData o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static TeamLevelBonusBuffOptionData Parse(IDictionary d)
	{
		return Parse(d, new TeamLevelBonusBuffOptionData());
	}

	public static TeamLevelBonusBuffOptionData Parse(IDictionary d, TeamLevelBonusBuffOptionData o)
	{
		o.Index = BaseJsonMarshaler.ParseValueInt(d, "Index");
		o.BonusType = BaseJsonMarshaler.ParseEnum<EventBonusType>(d, "BonusType");
		o.BonusValue = BaseJsonMarshaler.ParseArrayCast(d, "BonusValue", BaseJsonMarshaler.CastToInt);
		o.ReinforcePriceType = BaseJsonMarshaler.ParseEnum<CurrencyType>(d, "ReinforcePriceType");
		o.ReinforcePrice = BaseJsonMarshaler.ParseArrayCast(d, "ReinforcePrice", BaseJsonMarshaler.CastToInt);
		o.ConditionType = BaseJsonMarshaler.ParseEnum<TeamLevelBuffOptionConditionType>(d, "ConditionType");
		o.ConditionValue = BaseJsonMarshaler.ParseArrayCast(d, "ConditionValue", BaseJsonMarshaler.CastToString);
		o.ContentsOptionDescKey = BaseJsonMarshaler.ParseValueString(d, "ContentsOptionDescKey");
		o.Grade = BaseJsonMarshaler.ParseEnum<TeamLevelGrade>(d, "Grade");
		o.BonusNameKey = BaseJsonMarshaler.ParseValueString(d, "BonusNameKey");
		o.BonusDescKey = BaseJsonMarshaler.ParseValueString(d, "BonusDescKey");
		return o;
	}

	public static string EncodeJsonString(TeamLevelBonusBuffOptionData o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(TeamLevelBonusBuffOptionData o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("Index", o.Index.ToString());
		dictionary.Add("BonusType", o.BonusType.ToString());
		dictionary.Add("BonusValue", BaseJsonMarshaler.EncodeArray(o.BonusValue));
		dictionary.Add("ReinforcePriceType", o.ReinforcePriceType.ToString());
		dictionary.Add("ReinforcePrice", BaseJsonMarshaler.EncodeArray(o.ReinforcePrice));
		dictionary.Add("ConditionType", o.ConditionType.ToString());
		dictionary.Add("ConditionValue", BaseJsonMarshaler.EncodeArray(o.ConditionValue));
		if (o.ContentsOptionDescKey != null)
		{
			dictionary.Add("ContentsOptionDescKey", o.ContentsOptionDescKey);
		}
		dictionary.Add("Grade", o.Grade.ToString());
		if (o.BonusNameKey != null)
		{
			dictionary.Add("BonusNameKey", o.BonusNameKey);
		}
		if (o.BonusDescKey != null)
		{
			dictionary.Add("BonusDescKey", o.BonusDescKey);
		}
		return dictionary;
	}
}
