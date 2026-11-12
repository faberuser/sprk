using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_VideoData
{
	public static VideoData Parse(string s)
	{
		return Parse(s, new VideoData());
	}

	public static VideoData Parse(string s, VideoData o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static VideoData Parse(IDictionary d)
	{
		return Parse(d, new VideoData());
	}

	public static VideoData Parse(IDictionary d, VideoData o)
	{
		o.Index = BaseJsonMarshaler.ParseValueInt(d, "Index");
		o.Path = BaseJsonMarshaler.ParseValueString(d, "Path");
		o.Code = BaseJsonMarshaler.ParseValueString(d, "Code");
		return o;
	}

	public static string EncodeJsonString(VideoData o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(VideoData o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("Index", o.Index.ToString());
		if (o.Path != null)
		{
			dictionary.Add("Path", o.Path);
		}
		if (o.Code != null)
		{
			dictionary.Add("Code", o.Code);
		}
		return dictionary;
	}
}
