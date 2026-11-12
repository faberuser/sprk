using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.RejectJoinRequest;

public static class JM_NShared_RejectJoinRequest_Request
{
	public static NShared.RejectJoinRequest.Request Parse(string s)
	{
		return Parse(s, new NShared.RejectJoinRequest.Request());
	}

	public static NShared.RejectJoinRequest.Request Parse(string s, NShared.RejectJoinRequest.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.RejectJoinRequest.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.RejectJoinRequest.Request());
	}

	public static NShared.RejectJoinRequest.Request Parse(IDictionary d, NShared.RejectJoinRequest.Request o)
	{
		o.AccountId = BaseJsonMarshaler.ParseValueLong(d, "AccountId");
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.RejectJoinRequest.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.RejectJoinRequest.Request o)
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
