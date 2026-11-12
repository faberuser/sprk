using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.SearchUser;

public static class JM_NShared_SearchUser_Request
{
	public static NShared.SearchUser.Request Parse(string s)
	{
		return Parse(s, new NShared.SearchUser.Request());
	}

	public static NShared.SearchUser.Request Parse(string s, NShared.SearchUser.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.SearchUser.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.SearchUser.Request());
	}

	public static NShared.SearchUser.Request Parse(IDictionary d, NShared.SearchUser.Request o)
	{
		o.SearchNick = BaseJsonMarshaler.ParseValueString(d, "SearchNick");
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.SearchUser.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.SearchUser.Request o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		if (o.SearchNick != null)
		{
			dictionary.Add("SearchNick", o.SearchNick);
		}
		if (o.SessionId != null)
		{
			dictionary.Add("SessionId", o.SessionId);
		}
		return dictionary;
	}
}
