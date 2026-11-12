using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.NMessage;

public static class JM_NShared_NMessage_ChangeFriendReq
{
	public static ChangeFriendReq Parse(string s)
	{
		return Parse(s, new ChangeFriendReq());
	}

	public static ChangeFriendReq Parse(string s, ChangeFriendReq o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static ChangeFriendReq Parse(IDictionary d)
	{
		return Parse(d, new ChangeFriendReq());
	}

	public static ChangeFriendReq Parse(IDictionary d, ChangeFriendReq o)
	{
		o.FriendIds = BaseJsonMarshaler.ParseArrayCast(d, "FriendIds", BaseJsonMarshaler.CastToLong);
		o.RequestId = BaseJsonMarshaler.ParseValueInt(d, "RequestId");
		return o;
	}

	public static string EncodeJsonString(ChangeFriendReq o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(ChangeFriendReq o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("FriendIds", BaseJsonMarshaler.EncodeArray(o.FriendIds));
		dictionary.Add("RequestId", o.RequestId.ToString());
		return dictionary;
	}
}
