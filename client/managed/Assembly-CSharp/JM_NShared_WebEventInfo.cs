using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_WebEventInfo
{
	public static WebEventInfo Parse(string s)
	{
		return Parse(s, new WebEventInfo());
	}

	public static WebEventInfo Parse(string s, WebEventInfo o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static WebEventInfo Parse(IDictionary d)
	{
		return Parse(d, new WebEventInfo());
	}

	public static WebEventInfo Parse(IDictionary d, WebEventInfo o)
	{
		o.ImageUrl = BaseJsonMarshaler.ParseValueString(d, "ImageUrl");
		o.LinkUrl = BaseJsonMarshaler.ParseValueString(d, "LinkUrl");
		o.FirstEventId = BaseJsonMarshaler.ParseValueInt(d, "FirstEventId");
		o.QueryString = BaseJsonMarshaler.ParseValueString(d, "QueryString");
		o.WebContents = BaseJsonMarshaler.ParseArray(d, "WebContents", JM_NShared_WebContentInfo.Parse);
		return o;
	}

	public static string EncodeJsonString(WebEventInfo o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(WebEventInfo o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		if (o.ImageUrl != null)
		{
			dictionary.Add("ImageUrl", o.ImageUrl);
		}
		if (o.LinkUrl != null)
		{
			dictionary.Add("LinkUrl", o.LinkUrl);
		}
		dictionary.Add("FirstEventId", o.FirstEventId.ToString());
		if (o.QueryString != null)
		{
			dictionary.Add("QueryString", o.QueryString);
		}
		dictionary.Add("WebContents", BaseJsonMarshaler.EncodeArray(o.WebContents, JM_NShared_WebContentInfo.EncodeJsonObject));
		return dictionary;
	}
}
