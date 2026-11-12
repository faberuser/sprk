using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_LobbyNotice
{
	public static LobbyNotice Parse(string s)
	{
		return Parse(s, new LobbyNotice());
	}

	public static LobbyNotice Parse(string s, LobbyNotice o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static LobbyNotice Parse(IDictionary d)
	{
		return Parse(d, new LobbyNotice());
	}

	public static LobbyNotice Parse(IDictionary d, LobbyNotice o)
	{
		o.Index = BaseJsonMarshaler.ParseValueInt(d, "Index");
		o.Name = BaseJsonMarshaler.ParseValueString(d, "Name");
		o.BeginTime = BaseJsonMarshaler.ParseValueString(d, "BeginTime");
		o.EndTime = BaseJsonMarshaler.ParseValueString(d, "EndTime");
		o.LocalInfo = BaseJsonMarshaler.ParseArray(d, "LocalInfo", JM_NShared_LobbyNoticeLocalInfo.Parse);
		o.TaskInfo = BaseJsonMarshaler.ParseArray(d, "TaskInfo", JM_NShared_LobbyNoticeTaskInfo.Parse);
		return o;
	}

	public static string EncodeJsonString(LobbyNotice o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(LobbyNotice o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("Index", o.Index.ToString());
		if (o.Name != null)
		{
			dictionary.Add("Name", o.Name);
		}
		if (o.BeginTime != null)
		{
			dictionary.Add("BeginTime", o.BeginTime);
		}
		if (o.EndTime != null)
		{
			dictionary.Add("EndTime", o.EndTime);
		}
		dictionary.Add("LocalInfo", BaseJsonMarshaler.EncodeArray(o.LocalInfo, JM_NShared_LobbyNoticeLocalInfo.EncodeJsonObject));
		dictionary.Add("TaskInfo", BaseJsonMarshaler.EncodeArray(o.TaskInfo, JM_NShared_LobbyNoticeTaskInfo.EncodeJsonObject));
		return dictionary;
	}
}
