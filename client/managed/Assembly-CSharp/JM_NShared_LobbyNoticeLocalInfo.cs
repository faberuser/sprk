using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_LobbyNoticeLocalInfo
{
	public static LobbyNoticeLocalInfo Parse(string s)
	{
		return Parse(s, new LobbyNoticeLocalInfo());
	}

	public static LobbyNoticeLocalInfo Parse(string s, LobbyNoticeLocalInfo o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static LobbyNoticeLocalInfo Parse(IDictionary d)
	{
		return Parse(d, new LobbyNoticeLocalInfo());
	}

	public static LobbyNoticeLocalInfo Parse(IDictionary d, LobbyNoticeLocalInfo o)
	{
		o.Language = BaseJsonMarshaler.ParseValueString(d, "Language");
		o.ImageUrl = BaseJsonMarshaler.ParseValueString(d, "ImageUrl");
		o.LinkUrl = BaseJsonMarshaler.ParseValueString(d, "LinkUrl");
		o.Message = BaseJsonMarshaler.ParseValueString(d, "Message");
		return o;
	}

	public static string EncodeJsonString(LobbyNoticeLocalInfo o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(LobbyNoticeLocalInfo o)
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
		if (o.ImageUrl != null)
		{
			dictionary.Add("ImageUrl", o.ImageUrl);
		}
		if (o.LinkUrl != null)
		{
			dictionary.Add("LinkUrl", o.LinkUrl);
		}
		if (o.Message != null)
		{
			dictionary.Add("Message", o.Message);
		}
		return dictionary;
	}
}
