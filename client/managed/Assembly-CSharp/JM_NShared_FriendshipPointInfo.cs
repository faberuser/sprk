using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_FriendshipPointInfo
{
	public static FriendshipPointInfo Parse(string s)
	{
		return Parse(s, new FriendshipPointInfo());
	}

	public static FriendshipPointInfo Parse(string s, FriendshipPointInfo o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static FriendshipPointInfo Parse(IDictionary d)
	{
		return Parse(d, new FriendshipPointInfo());
	}

	public static FriendshipPointInfo Parse(IDictionary d, FriendshipPointInfo o)
	{
		o.FriendId = BaseJsonMarshaler.ParseValueLong(d, "FriendId");
		o.LastSendTime = BaseJsonMarshaler.ParseValueString(d, "LastSendTime");
		o.LastRecvTime = BaseJsonMarshaler.ParseValueString(d, "LastRecvTime");
		return o;
	}

	public static string EncodeJsonString(FriendshipPointInfo o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(FriendshipPointInfo o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("FriendId", o.FriendId.ToString());
		if (o.LastSendTime != null)
		{
			dictionary.Add("LastSendTime", o.LastSendTime);
		}
		if (o.LastRecvTime != null)
		{
			dictionary.Add("LastRecvTime", o.LastRecvTime);
		}
		return dictionary;
	}
}
