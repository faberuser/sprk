using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.GetEventStepReward;

public static class JM_NShared_GetEventStepReward_Request
{
	public static NShared.GetEventStepReward.Request Parse(string s)
	{
		return Parse(s, new NShared.GetEventStepReward.Request());
	}

	public static NShared.GetEventStepReward.Request Parse(string s, NShared.GetEventStepReward.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.GetEventStepReward.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.GetEventStepReward.Request());
	}

	public static NShared.GetEventStepReward.Request Parse(IDictionary d, NShared.GetEventStepReward.Request o)
	{
		o.EventStepType = BaseJsonMarshaler.ParseEnum<EventStepType>(d, "EventStepType");
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.GetEventStepReward.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.GetEventStepReward.Request o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("EventStepType", o.EventStepType.ToString());
		if (o.SessionId != null)
		{
			dictionary.Add("SessionId", o.SessionId);
		}
		return dictionary;
	}
}
