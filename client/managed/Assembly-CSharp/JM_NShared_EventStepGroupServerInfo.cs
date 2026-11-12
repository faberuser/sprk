using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_EventStepGroupServerInfo
{
	public static EventStepGroupServerInfo Parse(string s)
	{
		return Parse(s, new EventStepGroupServerInfo());
	}

	public static EventStepGroupServerInfo Parse(string s, EventStepGroupServerInfo o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static EventStepGroupServerInfo Parse(IDictionary d)
	{
		return Parse(d, new EventStepGroupServerInfo());
	}

	public static EventStepGroupServerInfo Parse(IDictionary d, EventStepGroupServerInfo o)
	{
		o.Step = BaseJsonMarshaler.ParseValueInt(d, "Step");
		o.TargetScore = BaseJsonMarshaler.ParseValueInt(d, "TargetScore");
		return o;
	}

	public static string EncodeJsonString(EventStepGroupServerInfo o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(EventStepGroupServerInfo o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("Step", o.Step.ToString());
		dictionary.Add("TargetScore", o.TargetScore.ToString());
		return dictionary;
	}
}
