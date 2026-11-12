using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.NMessage.NProtocol;

public static class JM_NShared_NMessage_NProtocol_SentFriendshipPoint
{
	public static SentFriendshipPoint Parse(string s)
	{
		return Parse(s, new SentFriendshipPoint());
	}

	public static SentFriendshipPoint Parse(string s, SentFriendshipPoint o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static SentFriendshipPoint Parse(IDictionary d)
	{
		return Parse(d, new SentFriendshipPoint());
	}

	public static SentFriendshipPoint Parse(IDictionary d, SentFriendshipPoint o)
	{
		o.SentFriendshipPointInfo = BaseJsonMarshaler.ParseObject(d, "SentFriendshipPointInfo", JM_NShared_FriendshipPointInfo.Parse);
		return o;
	}

	public static string EncodeJsonString(SentFriendshipPoint o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(SentFriendshipPoint o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("SentFriendshipPointInfo", JM_NShared_FriendshipPointInfo.EncodeJsonObject(o.SentFriendshipPointInfo));
		return dictionary;
	}
}
