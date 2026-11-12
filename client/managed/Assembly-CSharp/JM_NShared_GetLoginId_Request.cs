using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.GetLoginId;

public static class JM_NShared_GetLoginId_Request
{
	public static NShared.GetLoginId.Request Parse(string s)
	{
		return Parse(s, new NShared.GetLoginId.Request());
	}

	public static NShared.GetLoginId.Request Parse(string s, NShared.GetLoginId.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.GetLoginId.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.GetLoginId.Request());
	}

	public static NShared.GetLoginId.Request Parse(IDictionary d, NShared.GetLoginId.Request o)
	{
		o.AccountId = BaseJsonMarshaler.ParseValueLong(d, "AccountId");
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.GetLoginId.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.GetLoginId.Request o)
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
