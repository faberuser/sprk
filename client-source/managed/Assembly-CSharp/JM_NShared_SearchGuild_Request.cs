using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.SearchGuild;

public static class JM_NShared_SearchGuild_Request
{
	public static NShared.SearchGuild.Request Parse(string s)
	{
		return Parse(s, new NShared.SearchGuild.Request());
	}

	public static NShared.SearchGuild.Request Parse(string s, NShared.SearchGuild.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.SearchGuild.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.SearchGuild.Request());
	}

	public static NShared.SearchGuild.Request Parse(IDictionary d, NShared.SearchGuild.Request o)
	{
		o.name = BaseJsonMarshaler.ParseValueString(d, "name");
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.SearchGuild.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.SearchGuild.Request o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		if (o.name != null)
		{
			dictionary.Add("name", o.name);
		}
		if (o.SessionId != null)
		{
			dictionary.Add("SessionId", o.SessionId);
		}
		return dictionary;
	}
}
