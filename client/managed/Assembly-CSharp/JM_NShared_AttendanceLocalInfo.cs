using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_AttendanceLocalInfo
{
	public static AttendanceLocalInfo Parse(string s)
	{
		return Parse(s, new AttendanceLocalInfo());
	}

	public static AttendanceLocalInfo Parse(string s, AttendanceLocalInfo o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static AttendanceLocalInfo Parse(IDictionary d)
	{
		return Parse(d, new AttendanceLocalInfo());
	}

	public static AttendanceLocalInfo Parse(IDictionary d, AttendanceLocalInfo o)
	{
		o.Index = BaseJsonMarshaler.ParseValueInt(d, "Index");
		o.Language = BaseJsonMarshaler.ParseValueString(d, "Language");
		o.Title = BaseJsonMarshaler.ParseValueString(d, "Title");
		o.BackgroundImageUrl = BaseJsonMarshaler.ParseValueString(d, "BackgroundImageUrl");
		o.DescTextKey = BaseJsonMarshaler.ParseValueString(d, "DescTextKey");
		return o;
	}

	public static string EncodeJsonString(AttendanceLocalInfo o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(AttendanceLocalInfo o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("Index", o.Index.ToString());
		if (o.Language != null)
		{
			dictionary.Add("Language", o.Language);
		}
		if (o.Title != null)
		{
			dictionary.Add("Title", o.Title);
		}
		if (o.BackgroundImageUrl != null)
		{
			dictionary.Add("BackgroundImageUrl", o.BackgroundImageUrl);
		}
		if (o.DescTextKey != null)
		{
			dictionary.Add("DescTextKey", o.DescTextKey);
		}
		return dictionary;
	}
}
