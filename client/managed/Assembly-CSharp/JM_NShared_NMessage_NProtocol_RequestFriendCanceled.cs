using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared.NMessage.NProtocol;

public static class JM_NShared_NMessage_NProtocol_RequestFriendCanceled
{
	public static RequestFriendCanceled Parse(string s)
	{
		return Parse(s, new RequestFriendCanceled());
	}

	public static RequestFriendCanceled Parse(string s, RequestFriendCanceled o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static RequestFriendCanceled Parse(IDictionary d)
	{
		return Parse(d, new RequestFriendCanceled());
	}

	public static RequestFriendCanceled Parse(IDictionary d, RequestFriendCanceled o)
	{
		return o;
	}

	public static string EncodeJsonString(RequestFriendCanceled o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(RequestFriendCanceled o)
	{
		return new Dictionary<string, object>();
	}
}
