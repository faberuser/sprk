using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.SearchFriend;

public static class JM_NShared_SearchFriend_Request
{
	public static NShared.SearchFriend.Request Parse(string s)
	{
		return Parse(s, new NShared.SearchFriend.Request());
	}

	public static NShared.SearchFriend.Request Parse(string s, NShared.SearchFriend.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.SearchFriend.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.SearchFriend.Request());
	}

	public static NShared.SearchFriend.Request Parse(IDictionary d, NShared.SearchFriend.Request o)
	{
		o.Keyword = BaseJsonMarshaler.ParseValueString(d, "Keyword");
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.SearchFriend.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.SearchFriend.Request o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		if (o.Keyword != null)
		{
			dictionary.Add("Keyword", o.Keyword);
		}
		if (o.SessionId != null)
		{
			dictionary.Add("SessionId", o.SessionId);
		}
		return dictionary;
	}
}
