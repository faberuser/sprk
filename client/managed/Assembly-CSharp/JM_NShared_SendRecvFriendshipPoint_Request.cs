using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.SendRecvFriendshipPoint;

public static class JM_NShared_SendRecvFriendshipPoint_Request
{
	public static NShared.SendRecvFriendshipPoint.Request Parse(string s)
	{
		return Parse(s, new NShared.SendRecvFriendshipPoint.Request());
	}

	public static NShared.SendRecvFriendshipPoint.Request Parse(string s, NShared.SendRecvFriendshipPoint.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.SendRecvFriendshipPoint.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.SendRecvFriendshipPoint.Request());
	}

	public static NShared.SendRecvFriendshipPoint.Request Parse(IDictionary d, NShared.SendRecvFriendshipPoint.Request o)
	{
		o.SendFriendIds = BaseJsonMarshaler.ParseArrayCast(d, "SendFriendIds", BaseJsonMarshaler.CastToLong);
		o.RecvFriendIds = BaseJsonMarshaler.ParseArrayCast(d, "RecvFriendIds", BaseJsonMarshaler.CastToLong);
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.SendRecvFriendshipPoint.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.SendRecvFriendshipPoint.Request o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("SendFriendIds", BaseJsonMarshaler.EncodeArray(o.SendFriendIds));
		dictionary.Add("RecvFriendIds", BaseJsonMarshaler.EncodeArray(o.RecvFriendIds));
		if (o.SessionId != null)
		{
			dictionary.Add("SessionId", o.SessionId);
		}
		return dictionary;
	}
}
