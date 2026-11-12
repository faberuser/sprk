using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_LeftMenuButtonData
{
	public static LeftMenuButtonData Parse(string s)
	{
		return Parse(s, new LeftMenuButtonData());
	}

	public static LeftMenuButtonData Parse(string s, LeftMenuButtonData o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static LeftMenuButtonData Parse(IDictionary d)
	{
		return Parse(d, new LeftMenuButtonData());
	}

	public static LeftMenuButtonData Parse(IDictionary d, LeftMenuButtonData o)
	{
		o.Index = BaseJsonMarshaler.ParseValueInt(d, "Index");
		o.LeftMenuButtonType = BaseJsonMarshaler.ParseEnum<LeftMenuButtonType>(d, "LeftMenuButtonType");
		o.SubType = BaseJsonMarshaler.ParseValueString(d, "SubType");
		o.ShowIndex = BaseJsonMarshaler.ParseValueInt(d, "ShowIndex");
		o.TaskName = BaseJsonMarshaler.ParseValueString(d, "TaskName");
		o.TaskValue = BaseJsonMarshaler.ParseArrayCast(d, "TaskValue", BaseJsonMarshaler.CastToString);
		return o;
	}

	public static string EncodeJsonString(LeftMenuButtonData o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(LeftMenuButtonData o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("Index", o.Index.ToString());
		dictionary.Add("LeftMenuButtonType", o.LeftMenuButtonType.ToString());
		if (o.SubType != null)
		{
			dictionary.Add("SubType", o.SubType);
		}
		dictionary.Add("ShowIndex", o.ShowIndex.ToString());
		if (o.TaskName != null)
		{
			dictionary.Add("TaskName", o.TaskName);
		}
		dictionary.Add("TaskValue", BaseJsonMarshaler.EncodeArray(o.TaskValue));
		return dictionary;
	}
}
