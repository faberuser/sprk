using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.RequestFriend;

public static class JM_NShared_RequestFriend_Request
{
	public static NShared.RequestFriend.Request Parse(string s)
	{
		return Parse(s, new NShared.RequestFriend.Request());
	}

	public static NShared.RequestFriend.Request Parse(string s, NShared.RequestFriend.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.RequestFriend.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.RequestFriend.Request());
	}

	public static NShared.RequestFriend.Request Parse(IDictionary d, NShared.RequestFriend.Request o)
	{
		o.FriendId = BaseJsonMarshaler.ParseValueLong(d, "FriendId");
		o.InviteType = BaseJsonMarshaler.ParseEnum<FriendInviteType>(d, "InviteType");
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.RequestFriend.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.RequestFriend.Request o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("FriendId", o.FriendId.ToString());
		dictionary.Add("InviteType", o.InviteType.ToString());
		if (o.SessionId != null)
		{
			dictionary.Add("SessionId", o.SessionId);
		}
		return dictionary;
	}
}
