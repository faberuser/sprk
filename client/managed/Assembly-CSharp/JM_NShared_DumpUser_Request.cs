using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.DumpUser;

public static class JM_NShared_DumpUser_Request
{
	public static NShared.DumpUser.Request Parse(string s)
	{
		return Parse(s, new NShared.DumpUser.Request());
	}

	public static NShared.DumpUser.Request Parse(string s, NShared.DumpUser.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.DumpUser.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.DumpUser.Request());
	}

	public static NShared.DumpUser.Request Parse(IDictionary d, NShared.DumpUser.Request o)
	{
		o.AccountId = BaseJsonMarshaler.ParseValueInt(d, "AccountId");
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.DumpUser.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.DumpUser.Request o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("AccountId", o.AccountId.ToString());
		if (o.SessionId != null)
		{
			dictionary.Add("SessionId", o.SessionId);
		}
		return dictionary;
	}
}
