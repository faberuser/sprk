using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_PlayerTaskInfo
{
	public static PlayerTaskInfo Parse(string s)
	{
		return Parse(s, new PlayerTaskInfo());
	}

	public static PlayerTaskInfo Parse(string s, PlayerTaskInfo o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static PlayerTaskInfo Parse(IDictionary d)
	{
		return Parse(d, new PlayerTaskInfo());
	}

	public static PlayerTaskInfo Parse(IDictionary d, PlayerTaskInfo o)
	{
		o.TaskKey = BaseJsonMarshaler.ParseEnum<PlayerTaskType>(d, "TaskKey");
		o.HandlerName = BaseJsonMarshaler.ParseValueString(d, "HandlerName");
		o.State = BaseJsonMarshaler.ParseEnum<PlayerTaskState>(d, "State");
		o.BeginTime = BaseJsonMarshaler.ParseValueString(d, "BeginTime");
		o.EndTime = BaseJsonMarshaler.ParseValueString(d, "EndTime");
		o.UpdatedTime = BaseJsonMarshaler.ParseValueString(d, "UpdatedTime");
		o.FailCount = BaseJsonMarshaler.ParseValueInt(d, "FailCount");
		o.ExtraInfo = BaseJsonMarshaler.ParseValueString(d, "ExtraInfo");
		return o;
	}

	public static string EncodeJsonString(PlayerTaskInfo o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(PlayerTaskInfo o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("TaskKey", o.TaskKey.ToString());
		if (o.HandlerName != null)
		{
			dictionary.Add("HandlerName", o.HandlerName);
		}
		dictionary.Add("State", o.State.ToString());
		if (o.BeginTime != null)
		{
			dictionary.Add("BeginTime", o.BeginTime);
		}
		if (o.EndTime != null)
		{
			dictionary.Add("EndTime", o.EndTime);
		}
		if (o.UpdatedTime != null)
		{
			dictionary.Add("UpdatedTime", o.UpdatedTime);
		}
		dictionary.Add("FailCount", o.FailCount.ToString());
		if (o.ExtraInfo != null)
		{
			dictionary.Add("ExtraInfo", o.ExtraInfo);
		}
		return dictionary;
	}
}
