using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.AcceptJoinRequest;

public static class JM_NShared_AcceptJoinRequest_Request
{
	public static NShared.AcceptJoinRequest.Request Parse(string s)
	{
		return Parse(s, new NShared.AcceptJoinRequest.Request());
	}

	public static NShared.AcceptJoinRequest.Request Parse(string s, NShared.AcceptJoinRequest.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.AcceptJoinRequest.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.AcceptJoinRequest.Request());
	}

	public static NShared.AcceptJoinRequest.Request Parse(IDictionary d, NShared.AcceptJoinRequest.Request o)
	{
		o.AccountId = BaseJsonMarshaler.ParseValueLong(d, "AccountId");
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.AcceptJoinRequest.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.AcceptJoinRequest.Request o)
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
