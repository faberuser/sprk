using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.AutoGenerateQuery;

public static class JM_NShared_AutoGenerateQuery_Request
{
	public static NShared.AutoGenerateQuery.Request Parse(string s)
	{
		return Parse(s, new NShared.AutoGenerateQuery.Request());
	}

	public static NShared.AutoGenerateQuery.Request Parse(string s, NShared.AutoGenerateQuery.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.AutoGenerateQuery.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.AutoGenerateQuery.Request());
	}

	public static NShared.AutoGenerateQuery.Request Parse(IDictionary d, NShared.AutoGenerateQuery.Request o)
	{
		o.AccountId = BaseJsonMarshaler.ParseValueInt(d, "AccountId");
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.AutoGenerateQuery.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.AutoGenerateQuery.Request o)
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
