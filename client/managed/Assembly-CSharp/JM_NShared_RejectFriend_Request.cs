using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.RejectFriend;

public static class JM_NShared_RejectFriend_Request
{
	public static NShared.RejectFriend.Request Parse(string s)
	{
		return Parse(s, new NShared.RejectFriend.Request());
	}

	public static NShared.RejectFriend.Request Parse(string s, NShared.RejectFriend.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.RejectFriend.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.RejectFriend.Request());
	}

	public static NShared.RejectFriend.Request Parse(IDictionary d, NShared.RejectFriend.Request o)
	{
		o.FriendId = BaseJsonMarshaler.ParseValueLong(d, "FriendId");
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.RejectFriend.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.RejectFriend.Request o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("FriendId", o.FriendId.ToString());
		if (o.SessionId != null)
		{
			dictionary.Add("SessionId", o.SessionId);
		}
		return dictionary;
	}
}
