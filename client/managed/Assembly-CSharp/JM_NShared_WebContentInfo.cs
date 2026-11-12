using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_WebContentInfo
{
	public static WebContentInfo Parse(string s)
	{
		return Parse(s, new WebContentInfo());
	}

	public static WebContentInfo Parse(string s, WebContentInfo o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static WebContentInfo Parse(IDictionary d)
	{
		return Parse(d, new WebContentInfo());
	}

	public static WebContentInfo Parse(IDictionary d, WebContentInfo o)
	{
		o.Index = BaseJsonMarshaler.ParseValueInt(d, "Index");
		o.Title = BaseJsonMarshaler.ParseValueString(d, "Title");
		o.BeginTime = BaseJsonMarshaler.ParseValueString(d, "BeginTime");
		o.EndTime = BaseJsonMarshaler.ParseValueString(d, "EndTime");
		o.ImageUrl = BaseJsonMarshaler.ParseValueString(d, "ImageUrl");
		o.DisplayEvent = BaseJsonMarshaler.ParseValueByte(d, "DisplayEvent");
		o.DisplayOnTime = BaseJsonMarshaler.ParseValueString(d, "DisplayOnTime");
		o.DisplayOffTime = BaseJsonMarshaler.ParseValueString(d, "DisplayOffTime");
		o.HasAReward = BaseJsonMarshaler.ParseValueBool(d, "HasAReward");
		o.IsComplete = BaseJsonMarshaler.ParseValueBool(d, "IsComplete");
		return o;
	}

	public static string EncodeJsonString(WebContentInfo o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(WebContentInfo o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("Index", o.Index.ToString());
		if (o.Title != null)
		{
			dictionary.Add("Title", o.Title);
		}
		if (o.BeginTime != null)
		{
			dictionary.Add("BeginTime", o.BeginTime);
		}
		if (o.EndTime != null)
		{
			dictionary.Add("EndTime", o.EndTime);
		}
		if (o.ImageUrl != null)
		{
			dictionary.Add("ImageUrl", o.ImageUrl);
		}
		dictionary.Add("DisplayEvent", o.DisplayEvent.ToString());
		if (o.DisplayOnTime != null)
		{
			dictionary.Add("DisplayOnTime", o.DisplayOnTime);
		}
		if (o.DisplayOffTime != null)
		{
			dictionary.Add("DisplayOffTime", o.DisplayOffTime);
		}
		dictionary.Add("HasAReward", o.HasAReward.ToString());
		dictionary.Add("IsComplete", o.IsComplete.ToString());
		return dictionary;
	}
}
