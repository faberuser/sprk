using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_RouletteRewardInfo
{
	public static RouletteRewardInfo Parse(string s)
	{
		return Parse(s, new RouletteRewardInfo());
	}

	public static RouletteRewardInfo Parse(string s, RouletteRewardInfo o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static RouletteRewardInfo Parse(IDictionary d)
	{
		return Parse(d, new RouletteRewardInfo());
	}

	public static RouletteRewardInfo Parse(IDictionary d, RouletteRewardInfo o)
	{
		o.Index = BaseJsonMarshaler.ParseValueInt(d, "Index");
		o.Section = BaseJsonMarshaler.ParseValueInt(d, "Section");
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

	public static string EncodeJsonString(RouletteRewardInfo o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(RouletteRewardInfo o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("Index", o.Index.ToString());
		dictionary.Add("Section", o.Section.ToString());
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
