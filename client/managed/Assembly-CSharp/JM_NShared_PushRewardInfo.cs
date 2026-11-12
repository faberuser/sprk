using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_PushRewardInfo
{
	public static PushRewardInfo Parse(string s)
	{
		return Parse(s, new PushRewardInfo());
	}

	public static PushRewardInfo Parse(string s, PushRewardInfo o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static PushRewardInfo Parse(IDictionary d)
	{
		return Parse(d, new PushRewardInfo());
	}

	public static PushRewardInfo Parse(IDictionary d, PushRewardInfo o)
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

	public static string EncodeJsonString(PushRewardInfo o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(PushRewardInfo o)
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
