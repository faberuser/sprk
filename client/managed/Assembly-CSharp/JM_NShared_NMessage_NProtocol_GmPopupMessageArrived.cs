using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.NMessage.NProtocol;

public static class JM_NShared_NMessage_NProtocol_GmPopupMessageArrived
{
	public static GmPopupMessageArrived Parse(string s)
	{
		return Parse(s, new GmPopupMessageArrived());
	}

	public static GmPopupMessageArrived Parse(string s, GmPopupMessageArrived o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static GmPopupMessageArrived Parse(IDictionary d)
	{
		return Parse(d, new GmPopupMessageArrived());
	}

	public static GmPopupMessageArrived Parse(IDictionary d, GmPopupMessageArrived o)
	{
		o.Title = BaseJsonMarshaler.ParseValueString(d, "Title");
		o.Content = BaseJsonMarshaler.ParseValueString(d, "Content");
		return o;
	}

	public static string EncodeJsonString(GmPopupMessageArrived o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(GmPopupMessageArrived o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		if (o.Title != null)
		{
			dictionary.Add("Title", o.Title);
		}
		if (o.Content != null)
		{
			dictionary.Add("Content", o.Content);
		}
		return dictionary;
	}
}
