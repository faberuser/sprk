using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_DownloadControlData
{
	public static DownloadControlData Parse(string s)
	{
		return Parse(s, new DownloadControlData());
	}

	public static DownloadControlData Parse(string s, DownloadControlData o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static DownloadControlData Parse(IDictionary d)
	{
		return Parse(d, new DownloadControlData());
	}

	public static DownloadControlData Parse(IDictionary d, DownloadControlData o)
	{
		o.Index = BaseJsonMarshaler.ParseValueInt(d, "Index");
		o.ChapterIndex = BaseJsonMarshaler.ParseValueInt(d, "ChapterIndex");
		o.Categories = BaseJsonMarshaler.ParseArrayCast(d, "Categories", BaseJsonMarshaler.CastToString);
		return o;
	}

	public static string EncodeJsonString(DownloadControlData o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(DownloadControlData o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("Index", o.Index.ToString());
		dictionary.Add("ChapterIndex", o.ChapterIndex.ToString());
		dictionary.Add("Categories", BaseJsonMarshaler.EncodeArray(o.Categories));
		return dictionary;
	}
}
