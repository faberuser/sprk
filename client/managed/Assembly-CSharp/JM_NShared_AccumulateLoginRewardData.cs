using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_AccumulateLoginRewardData
{
	public static AccumulateLoginRewardData Parse(string s)
	{
		return Parse(s, new AccumulateLoginRewardData());
	}

	public static AccumulateLoginRewardData Parse(string s, AccumulateLoginRewardData o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static AccumulateLoginRewardData Parse(IDictionary d)
	{
		return Parse(d, new AccumulateLoginRewardData());
	}

	public static AccumulateLoginRewardData Parse(IDictionary d, AccumulateLoginRewardData o)
	{
		o.Index = BaseJsonMarshaler.ParseValueInt(d, "Index");
		o.Day = BaseJsonMarshaler.ParseValueInt(d, "Day");
		o.Type1 = BaseJsonMarshaler.ParseEnum<RewardType>(d, "Type1");
		o.Value1 = BaseJsonMarshaler.ParseValueInt(d, "Value1");
		o.Count1 = BaseJsonMarshaler.ParseValueInt(d, "Count1");
		o.Type2 = BaseJsonMarshaler.ParseEnum<RewardType>(d, "Type2");
		o.Value2 = BaseJsonMarshaler.ParseValueInt(d, "Value2");
		o.Count2 = BaseJsonMarshaler.ParseValueInt(d, "Count2");
		o.Type3 = BaseJsonMarshaler.ParseEnum<RewardType>(d, "Type3");
		o.Value3 = BaseJsonMarshaler.ParseValueInt(d, "Value3");
		o.Count3 = BaseJsonMarshaler.ParseValueInt(d, "Count3");
		return o;
	}

	public static string EncodeJsonString(AccumulateLoginRewardData o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(AccumulateLoginRewardData o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("Index", o.Index.ToString());
		dictionary.Add("Day", o.Day.ToString());
		dictionary.Add("Type1", o.Type1.ToString());
		dictionary.Add("Value1", o.Value1.ToString());
		dictionary.Add("Count1", o.Count1.ToString());
		dictionary.Add("Type2", o.Type2.ToString());
		dictionary.Add("Value2", o.Value2.ToString());
		dictionary.Add("Count2", o.Count2.ToString());
		dictionary.Add("Type3", o.Type3.ToString());
		dictionary.Add("Value3", o.Value3.ToString());
		dictionary.Add("Count3", o.Count3.ToString());
		return dictionary;
	}
}
