using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_TeamLevelBuffOptionData
{
	public static TeamLevelBuffOptionData Parse(string s)
	{
		return Parse(s, new TeamLevelBuffOptionData());
	}

	public static TeamLevelBuffOptionData Parse(string s, TeamLevelBuffOptionData o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static TeamLevelBuffOptionData Parse(IDictionary d)
	{
		return Parse(d, new TeamLevelBuffOptionData());
	}

	public static TeamLevelBuffOptionData Parse(IDictionary d, TeamLevelBuffOptionData o)
	{
		o.Index = BaseJsonMarshaler.ParseValueInt(d, "Index");
		o.OptionType = BaseJsonMarshaler.ParseEnum<OperationType>(d, "OptionType");
		o.OptionValue = BaseJsonMarshaler.ParseArrayCast(d, "OptionValue", BaseJsonMarshaler.CastToInt);
		o.ReinforcePriceType = BaseJsonMarshaler.ParseEnum<CurrencyType>(d, "ReinforcePriceType");
		o.ReinforcePrice = BaseJsonMarshaler.ParseArrayCast(d, "ReinforcePrice", BaseJsonMarshaler.CastToInt);
		o.ConditionType = BaseJsonMarshaler.ParseEnum<TeamLevelBuffOptionConditionType>(d, "ConditionType");
		o.ConditionValue = BaseJsonMarshaler.ParseArrayCast(d, "ConditionValue", BaseJsonMarshaler.CastToString);
		o.ContentsOptionDescKey = BaseJsonMarshaler.ParseValueString(d, "ContentsOptionDescKey");
		o.Grade = BaseJsonMarshaler.ParseEnum<TeamLevelGrade>(d, "Grade");
		return o;
	}

	public static string EncodeJsonString(TeamLevelBuffOptionData o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(TeamLevelBuffOptionData o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("Index", o.Index.ToString());
		dictionary.Add("OptionType", o.OptionType.ToString());
		dictionary.Add("OptionValue", BaseJsonMarshaler.EncodeArray(o.OptionValue));
		dictionary.Add("ReinforcePriceType", o.ReinforcePriceType.ToString());
		dictionary.Add("ReinforcePrice", BaseJsonMarshaler.EncodeArray(o.ReinforcePrice));
		dictionary.Add("ConditionType", o.ConditionType.ToString());
		dictionary.Add("ConditionValue", BaseJsonMarshaler.EncodeArray(o.ConditionValue));
		if (o.ContentsOptionDescKey != null)
		{
			dictionary.Add("ContentsOptionDescKey", o.ContentsOptionDescKey);
		}
		dictionary.Add("Grade", o.Grade.ToString());
		return dictionary;
	}
}
