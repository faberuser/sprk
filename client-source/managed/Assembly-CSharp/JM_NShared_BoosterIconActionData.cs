using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_BoosterIconActionData
{
	public static BoosterIconActionData Parse(string s)
	{
		return Parse(s, new BoosterIconActionData());
	}

	public static BoosterIconActionData Parse(string s, BoosterIconActionData o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static BoosterIconActionData Parse(IDictionary d)
	{
		return Parse(d, new BoosterIconActionData());
	}

	public static BoosterIconActionData Parse(IDictionary d, BoosterIconActionData o)
	{
		o.EventBonusType = BaseJsonMarshaler.ParseValueString(d, "EventBonusType");
		o.MoveTaskType = BaseJsonMarshaler.ParseValueString(d, "MoveTaskType");
		o.MoveTaskValue = BaseJsonMarshaler.ParseArrayCast(d, "MoveTaskValue", BaseJsonMarshaler.CastToString);
		return o;
	}

	public static string EncodeJsonString(BoosterIconActionData o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(BoosterIconActionData o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		if (o.EventBonusType != null)
		{
			dictionary.Add("EventBonusType", o.EventBonusType);
		}
		if (o.MoveTaskType != null)
		{
			dictionary.Add("MoveTaskType", o.MoveTaskType);
		}
		dictionary.Add("MoveTaskValue", BaseJsonMarshaler.EncodeArray(o.MoveTaskValue));
		return dictionary;
	}
}
