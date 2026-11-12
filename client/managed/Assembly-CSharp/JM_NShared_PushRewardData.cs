using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_PushRewardData
{
	public static PushRewardData Parse(string s)
	{
		return Parse(s, new PushRewardData());
	}

	public static PushRewardData Parse(string s, PushRewardData o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static PushRewardData Parse(IDictionary d)
	{
		return Parse(d, new PushRewardData());
	}

	public static PushRewardData Parse(IDictionary d, PushRewardData o)
	{
		o.Index = BaseJsonMarshaler.ParseValueInt(d, "Index");
		o.DayOfTheWeek = BaseJsonMarshaler.ParseValueInt(d, "DayOfTheWeek");
		o.OpenTime = BaseJsonMarshaler.ParseValueInt(d, "OpenTime");
		o.EndTime = BaseJsonMarshaler.ParseValueInt(d, "EndTime");
		o.TouchAction = BaseJsonMarshaler.ParseValueString(d, "TouchAction");
		o.RewardIndex = BaseJsonMarshaler.ParseValueInt(d, "RewardIndex");
		o.NameKey = BaseJsonMarshaler.ParseValueString(d, "NameKey");
		return o;
	}

	public static string EncodeJsonString(PushRewardData o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(PushRewardData o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("Index", o.Index.ToString());
		dictionary.Add("DayOfTheWeek", o.DayOfTheWeek.ToString());
		dictionary.Add("OpenTime", o.OpenTime.ToString());
		dictionary.Add("EndTime", o.EndTime.ToString());
		if (o.TouchAction != null)
		{
			dictionary.Add("TouchAction", o.TouchAction);
		}
		dictionary.Add("RewardIndex", o.RewardIndex.ToString());
		if (o.NameKey != null)
		{
			dictionary.Add("NameKey", o.NameKey);
		}
		return dictionary;
	}
}
