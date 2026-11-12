using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_FriendInfo
{
	public static FriendInfo Parse(string s)
	{
		return Parse(s, new FriendInfo());
	}

	public static FriendInfo Parse(string s, FriendInfo o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static FriendInfo Parse(IDictionary d)
	{
		return Parse(d, new FriendInfo());
	}

	public static FriendInfo Parse(IDictionary d, FriendInfo o)
	{
		o.FriendId = BaseJsonMarshaler.ParseValueLong(d, "FriendId");
		o.InvitedTime = BaseJsonMarshaler.ParseValueString(d, "InvitedTime");
		return o;
	}

	public static string EncodeJsonString(FriendInfo o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(FriendInfo o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("FriendId", o.FriendId.ToString());
		if (o.InvitedTime != null)
		{
			dictionary.Add("InvitedTime", o.InvitedTime);
		}
		return dictionary;
	}
}
