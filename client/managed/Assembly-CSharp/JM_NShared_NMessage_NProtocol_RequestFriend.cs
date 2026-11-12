using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.NMessage.NProtocol;

public static class JM_NShared_NMessage_NProtocol_RequestFriend
{
	public static RequestFriend Parse(string s)
	{
		return Parse(s, new RequestFriend());
	}

	public static RequestFriend Parse(string s, RequestFriend o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static RequestFriend Parse(IDictionary d)
	{
		return Parse(d, new RequestFriend());
	}

	public static RequestFriend Parse(IDictionary d, RequestFriend o)
	{
		o.InvitorInfo = BaseJsonMarshaler.ParseObject(d, "InvitorInfo", JM_NShared_FriendUserInfo.Parse);
		return o;
	}

	public static string EncodeJsonString(RequestFriend o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(RequestFriend o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("InvitorInfo", JM_NShared_FriendUserInfo.EncodeJsonObject(o.InvitorInfo));
		return dictionary;
	}
}
