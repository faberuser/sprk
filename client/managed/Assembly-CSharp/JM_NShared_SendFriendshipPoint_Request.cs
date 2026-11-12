using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.SendFriendshipPoint;

public static class JM_NShared_SendFriendshipPoint_Request
{
	public static NShared.SendFriendshipPoint.Request Parse(string s)
	{
		return Parse(s, new NShared.SendFriendshipPoint.Request());
	}

	public static NShared.SendFriendshipPoint.Request Parse(string s, NShared.SendFriendshipPoint.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.SendFriendshipPoint.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.SendFriendshipPoint.Request());
	}

	public static NShared.SendFriendshipPoint.Request Parse(IDictionary d, NShared.SendFriendshipPoint.Request o)
	{
		o.FriendIds = BaseJsonMarshaler.ParseArrayCast(d, "FriendIds", BaseJsonMarshaler.CastToLong);
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.SendFriendshipPoint.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.SendFriendshipPoint.Request o)
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
