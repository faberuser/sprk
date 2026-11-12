using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_LobbyNoticeTaskInfo
{
	public static LobbyNoticeTaskInfo Parse(string s)
	{
		return Parse(s, new LobbyNoticeTaskInfo());
	}

	public static LobbyNoticeTaskInfo Parse(string s, LobbyNoticeTaskInfo o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static LobbyNoticeTaskInfo Parse(IDictionary d)
	{
		return Parse(d, new LobbyNoticeTaskInfo());
	}

	public static LobbyNoticeTaskInfo Parse(IDictionary d, LobbyNoticeTaskInfo o)
	{
		o.Position = BaseJsonMarshaler.ParseValueInt(d, "Position");
		o.TaskName = BaseJsonMarshaler.ParseValueString(d, "TaskName");
		o.TaskValue = BaseJsonMarshaler.ParseValueString(d, "TaskValue");
		return o;
	}

	public static string EncodeJsonString(LobbyNoticeTaskInfo o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(LobbyNoticeTaskInfo o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("Position", o.Position.ToString());
		if (o.TaskName != null)
		{
			dictionary.Add("TaskName", o.TaskName);
		}
		if (o.TaskValue != null)
		{
			dictionary.Add("TaskValue", o.TaskValue);
		}
		return dictionary;
	}
}
