using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_NoticeLocalInfo
{
	public static NoticeLocalInfo Parse(string s)
	{
		return Parse(s, new NoticeLocalInfo());
	}

	public static NoticeLocalInfo Parse(string s, NoticeLocalInfo o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NoticeLocalInfo Parse(IDictionary d)
	{
		return Parse(d, new NoticeLocalInfo());
	}

	public static NoticeLocalInfo Parse(IDictionary d, NoticeLocalInfo o)
	{
		o.Language = BaseJsonMarshaler.ParseValueString(d, "Language");
		o.Message = BaseJsonMarshaler.ParseValueString(d, "Message");
		return o;
	}

	public static string EncodeJsonString(NoticeLocalInfo o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NoticeLocalInfo o)
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
		if (o.Message != null)
		{
			dictionary.Add("Message", o.Message);
		}
		return dictionary;
	}
}
