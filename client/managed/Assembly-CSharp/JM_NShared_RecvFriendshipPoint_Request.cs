using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.RecvFriendshipPoint;

public static class JM_NShared_RecvFriendshipPoint_Request
{
	public static NShared.RecvFriendshipPoint.Request Parse(string s)
	{
		return Parse(s, new NShared.RecvFriendshipPoint.Request());
	}

	public static NShared.RecvFriendshipPoint.Request Parse(string s, NShared.RecvFriendshipPoint.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.RecvFriendshipPoint.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.RecvFriendshipPoint.Request());
	}

	public static NShared.RecvFriendshipPoint.Request Parse(IDictionary d, NShared.RecvFriendshipPoint.Request o)
	{
		o.FriendIds = BaseJsonMarshaler.ParseArrayCast(d, "FriendIds", BaseJsonMarshaler.CastToLong);
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.RecvFriendshipPoint.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.RecvFriendshipPoint.Request o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("FriendIds", BaseJsonMarshaler.EncodeArray(o.FriendIds));
		if (o.SessionId != null)
		{
			dictionary.Add("SessionId", o.SessionId);
		}
		return dictionary;
	}
}
