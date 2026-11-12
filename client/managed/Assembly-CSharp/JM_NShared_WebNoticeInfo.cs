using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_WebNoticeInfo
{
	public static WebNoticeInfo Parse(string s)
	{
		return Parse(s, new WebNoticeInfo());
	}

	public static WebNoticeInfo Parse(string s, WebNoticeInfo o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static WebNoticeInfo Parse(IDictionary d)
	{
		return Parse(d, new WebNoticeInfo());
	}

	public static WebNoticeInfo Parse(IDictionary d, WebNoticeInfo o)
	{
		o.Language = BaseJsonMarshaler.ParseValueString(d, "Language");
		o.Url = BaseJsonMarshaler.ParseValueString(d, "Url");
		return o;
	}

	public static string EncodeJsonString(WebNoticeInfo o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(WebNoticeInfo o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		if (o.Language != null)
		{
			dictionary.Add("Language", o.Language);
		}
		if (o.Url != null)
		{
			dictionary.Add("Url", o.Url);
		}
		return dictionary;
	}
}
