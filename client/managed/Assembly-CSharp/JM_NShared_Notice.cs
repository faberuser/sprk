using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_Notice
{
	public static Notice Parse(string s)
	{
		return Parse(s, new Notice());
	}

	public static Notice Parse(string s, Notice o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static Notice Parse(IDictionary d)
	{
		return Parse(d, new Notice());
	}

	public static Notice Parse(IDictionary d, Notice o)
	{
		o.Id = BaseJsonMarshaler.ParseValueInt(d, "Id");
		o.BeginTime = BaseJsonMarshaler.ParseValueString(d, "BeginTime");
		o.EndTime = BaseJsonMarshaler.ParseValueString(d, "EndTime");
		o.Message = BaseJsonMarshaler.ParseValueString(d, "Message");
		o.Oneshot = BaseJsonMarshaler.ParseValueInt(d, "Oneshot");
		o.Interval = BaseJsonMarshaler.ParseValueInt(d, "Interval");
		o.LocalInfo = BaseJsonMarshaler.ParseArray(d, "LocalInfo", JM_NShared_NoticeLocalInfo.Parse);
		return o;
	}

	public static string EncodeJsonString(Notice o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(Notice o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("Id", o.Id.ToString());
		if (o.BeginTime != null)
		{
			dictionary.Add("BeginTime", o.BeginTime);
		}
		if (o.EndTime != null)
		{
			dictionary.Add("EndTime", o.EndTime);
		}
		if (o.Message != null)
		{
			dictionary.Add("Message", o.Message);
		}
		dictionary.Add("Oneshot", o.Oneshot.ToString());
		dictionary.Add("Interval", o.Interval.ToString());
		dictionary.Add("LocalInfo", BaseJsonMarshaler.EncodeArray(o.LocalInfo, JM_NShared_NoticeLocalInfo.EncodeJsonObject));
		return dictionary;
	}
}
