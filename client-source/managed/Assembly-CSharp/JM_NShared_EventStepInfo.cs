using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_EventStepInfo
{
	public static EventStepInfo Parse(string s)
	{
		return Parse(s, new EventStepInfo());
	}

	public static EventStepInfo Parse(string s, EventStepInfo o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static EventStepInfo Parse(IDictionary d)
	{
		return Parse(d, new EventStepInfo());
	}

	public static EventStepInfo Parse(IDictionary d, EventStepInfo o)
	{
		o.DailyStep = BaseJsonMarshaler.ParseValueInt(d, "DailyStep");
		o.DailyCount = BaseJsonMarshaler.ParseValueInt(d, "DailyCount");
		o.TotalCount = BaseJsonMarshaler.ParseValueInt(d, "TotalCount");
		o.MissionCount = BaseJsonMarshaler.ParseValueInt(d, "MissionCount");
		o.MissionStep = BaseJsonMarshaler.ParseValueInt(d, "MissionStep");
		o.GroupTotalCount = BaseJsonMarshaler.ParseValueInt(d, "GroupTotalCount");
		o.GroupTotalStep = BaseJsonMarshaler.ParseValueInt(d, "GroupTotalStep");
		o.GroupCurrentStep = BaseJsonMarshaler.ParseValueInt(d, "GroupCurrentStep");
		o.GroupTargetScore = BaseJsonMarshaler.ParseValueInt(d, "GroupTargetScore");
		o.GroupRemainScore = BaseJsonMarshaler.ParseValueInt(d, "GroupRemainScore");
		return o;
	}

	public static string EncodeJsonString(EventStepInfo o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(EventStepInfo o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("DailyStep", o.DailyStep.ToString());
		dictionary.Add("DailyCount", o.DailyCount.ToString());
		dictionary.Add("TotalCount", o.TotalCount.ToString());
		dictionary.Add("MissionCount", o.MissionCount.ToString());
		dictionary.Add("MissionStep", o.MissionStep.ToString());
		dictionary.Add("GroupTotalCount", o.GroupTotalCount.ToString());
		dictionary.Add("GroupTotalStep", o.GroupTotalStep.ToString());
		dictionary.Add("GroupCurrentStep", o.GroupCurrentStep.ToString());
		dictionary.Add("GroupTargetScore", o.GroupTargetScore.ToString());
		dictionary.Add("GroupRemainScore", o.GroupRemainScore.ToString());
		return dictionary;
	}
}
